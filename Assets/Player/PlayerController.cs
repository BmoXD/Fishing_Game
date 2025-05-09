using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System.Collections;

public class ThirdPersonController : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 2.0f;
    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 5.335f;
    [Tooltip("How fast the character turns to face movement direction")]
    public float RotationSmoothTime = 0.12f;
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.2f;
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;
    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Camera")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;
    [Tooltip("The Cinemachine Virtual Camera component")]
    public CinemachineVirtualCamera CinemachineCamera;
    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;
    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;
    [Tooltip("Enable/disable camera control for debugging")]
    public bool EnableCameraControl = true;
    [Tooltip("Minimum camera distance (zoom in limit)")]
    public float MinCameraDistance = 1.0f;
    [Tooltip("Maximum camera distance (zoom out limit)")]
    public float MaxCameraDistance = 10.0f;
    [Tooltip("How fast the camera zooms with scroll wheel")]
    public float ZoomSpeed = 1.0f;
    [Tooltip("Default camera distance")]
    public float DefaultCameraDistance = 5.0f;

    [Header("Sprint FOV Effect")]
    [Tooltip("How much FOV will be added when sprinting")]
    public float FOVSprintAddition = 10f;
    [Tooltip("How long it takes to add the extra FOV and go back to normal")]
    public float FOVSprintTime = 0.5f;
    [Tooltip("Curve for FOV interpolation (0-1)")]
    public AnimationCurve FOVCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private float _currentCameraDistance;
    private Cinemachine3rdPersonFollow _cinemachineFollowComponent;
    private float _defaultFOV;
    private float _targetFOV;
    private Coroutine _fovChangeCoroutine;
    private bool _wasSprintingLastFrame = false;

    // player
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;
    private bool _isRightMousePressed = false;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;
    private int _animIDToolEquipped;
    private int _animIDDefaultEquipped;

    private bool _playerControlsEnabled = true;
    private bool _movementEnabled = true;
    private PlayerControls _playerControls;
    private Animator _animator;
    private CharacterController _controller;
    private GameObject _mainCamera;
    private bool _hasAnimator;
    
    // grounded check
    private bool _grounded = true;
    private const float _threshold = 0.01f;

    private void Awake()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
        
        // Initialize input controls
        _playerControls = new PlayerControls();
        
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _hasAnimator = _animator != null;

        // Store initial camera rotation
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
        _cinemachineTargetPitch = CinemachineCameraTarget.transform.rotation.eulerAngles.x;

        // Get Cinemachine components
        if (CinemachineCamera != null)
        {
            _cinemachineFollowComponent = CinemachineCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            if (_cinemachineFollowComponent != null)
            {
                _currentCameraDistance = _cinemachineFollowComponent.CameraDistance;
            }
            else
            {
                _currentCameraDistance = DefaultCameraDistance;
                Debug.LogWarning("Cinemachine 3rd Person Follow component not found on the virtual camera.");
            }
            
            // Store default FOV
            _defaultFOV = CinemachineCamera.m_Lens.FieldOfView;
            _targetFOV = _defaultFOV;
        }
        else
        {
            _currentCameraDistance = DefaultCameraDistance;
            _defaultFOV = 60f; // Typical default FOV
            Debug.LogWarning("CinemachineCamera reference not set in the inspector.");
        }

        AssignAnimationIDs();

        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
    }

    private void Start()
    {
        // Initialize animation states
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDToolEquipped, false);
            _animator.SetBool(_animIDDefaultEquipped, false);
        }
        
        // Subscribe to inventory manager's equipped item changed event
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnEquippedItemChanged += HandleEquippedItemChanged;
        }
        else
        {
            Debug.LogWarning("InventoryManager instance not found. Equipment animations won't work.");
        }
    }

    private void OnEnable()
    {
        _playerControls.Enable();

        PlayerEvents.OnPlayerEnterMenu += HandlePlayerControlsChanged;
        
        // Subscribe to right mouse button events
        _playerControls.Player.RightMouse.performed += OnRightMousePressed;
        _playerControls.Player.RightMouse.canceled += OnRightMouseReleased;
        
        // Re-subscribe to inventory events if needed
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnEquippedItemChanged += HandleEquippedItemChanged;
        }

        PlayerEvents.OnFishingStateChanged += HandleFishingStateChanged;
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        _playerControls.Player.RightMouse.performed -= OnRightMousePressed;
        _playerControls.Player.RightMouse.canceled -= OnRightMouseReleased;

        PlayerEvents.OnPlayerEnterMenu -= HandlePlayerControlsChanged;
        
        // Unsubscribe from inventory events
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnEquippedItemChanged -= HandleEquippedItemChanged;
        }

        PlayerEvents.OnFishingStateChanged -= HandleFishingStateChanged;
        
        _playerControls.Disable();
        
        // Ensure cursor is visible when script is disabled
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    private void HandleFishingStateChanged(bool isFishing)
    {
        // Disable player movement controls, but keep camera movement enabled
        _movementEnabled = !isFishing;
            
        // Reset speed animation when fishing starts
        if (isFishing && _hasAnimator)
        {
            _animator.SetFloat("Speed", 0f);
        }
    }
    
    private void HandleEquippedItemChanged(InventoryItem newItem)
    {
        if (!_hasAnimator) return;
        
        // First, reset both animation parameters to false
        _animator.SetBool(_animIDToolEquipped, false);
        _animator.SetBool(_animIDDefaultEquipped, false);
        
        // If no item is equipped, we're done (both parameters remain false)
        if (newItem == null || newItem.ItemData == null)
        {
            return;
        }
        
        // Check the item type and set appropriate animation parameter
        switch (newItem.ItemData.type)
        {
            case ItemType.Tool:
                _animator.SetBool(_animIDToolEquipped, true);
                break;
                
            case ItemType.Default:
            case ItemType.Consumable:
                _animator.SetBool(_animIDDefaultEquipped, true);
                break;
        }
        
        // Optional debug log to verify the change happened
        Debug.Log($"Equipment changed to: {newItem.ItemData.itemName}, Type: {newItem.ItemData.type}");
    }

    private void HandlePlayerControlsChanged(bool enabled)
    {
        //Debug.Log("Before HandlePlayerControlsChanged: "+!enabled+" _playerControlsEnabled: "+ _playerControlsEnabled);
        _playerControlsEnabled = !enabled;
        //Debug.Log("After HandlePlayerControlsChanged: "+!enabled+" _playerControlsEnabled: "+ _playerControlsEnabled);

        if(enabled && _hasAnimator)
        {
            _animator.SetFloat("Speed", 0f);
        }
    }

    
    private void UpdateFOVForSprint(bool sprinting)
    {
        if (CinemachineCamera == null) return;
        
        _targetFOV = sprinting ? _defaultFOV + FOVSprintAddition : _defaultFOV;
        
        // Stop any existing FOV change coroutine
        if (_fovChangeCoroutine != null)
        {
            StopCoroutine(_fovChangeCoroutine);
        }
        
        // Start new FOV change coroutine
        _fovChangeCoroutine = StartCoroutine(ChangeFOV(_targetFOV));
    }
    
    private IEnumerator ChangeFOV(float targetFOV)
    {
        float startFOV = CinemachineCamera.m_Lens.FieldOfView;
        float timePassed = 0f;
        
        while (timePassed < FOVSprintTime)
        {
            timePassed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(timePassed / FOVSprintTime);
            float curveValue = FOVCurve.Evaluate(normalizedTime);
            
            CinemachineCamera.m_Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, curveValue);
            
            yield return null;
        }
        
        CinemachineCamera.m_Lens.FieldOfView = targetFOV;
    }
    
    private void OnRightMousePressed(InputAction.CallbackContext ctx)
    {
        Debug.Log("OnRightMousePressed _playerControlsEnabled: "+ _playerControlsEnabled);
        if (!_playerControlsEnabled)
        {
            Debug.Log("Can't move camera. Controls are disabled");
            return;
        }
        _isRightMousePressed = true;
        
        if (EnableCameraControl)
        {
            // Hide cursor and lock it to center of screen while camera is being moved
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    
    private void OnRightMouseReleased(InputAction.CallbackContext ctx)
    {
        if (!_playerControlsEnabled)
        {
            return;
        }
        _isRightMousePressed = false;
        
        // Show cursor and allow it to move freely when not moving camera
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");

        _animIDToolEquipped = Animator.StringToHash("ToolEquipped");
        _animIDDefaultEquipped = Animator.StringToHash("DefaultEquipped");
        // _animIDGrounded = Animator.StringToHash("Grounded");
        // _animIDJump = Animator.StringToHash("Jump");
        // _animIDFreeFall = Animator.StringToHash("FreeFall");
        // _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }
    private void Update()
    {
        if (!_playerControlsEnabled)
        {
            return;
        }
        GroundedCheck();
        JumpAndGravity();
        if (_movementEnabled)
        {
            Move();
            CheckSprintState();
        }
        HandleCameraZoom();
    }
    

    private void CheckSprintState()
    {
        // Get current sprint state directly from the input
        bool isCurrentlySprinting = _playerControls.Player.Sprint.IsPressed();
        Vector2 moveInput = _playerControls.Player.Move.ReadValue<Vector2>();
        bool isMoving = moveInput.sqrMagnitude > 0.1f;
        
        // Check for sprint state change
        if (isCurrentlySprinting && isMoving)
        {
            // Player is sprinting and moving - enable sprint FOV
            if (!_wasSprintingLastFrame)
            {
                UpdateFOVForSprint(true);
            }
        }
        else
        {
            // Player is not sprinting or not moving - disable sprint FOV
            if (_wasSprintingLastFrame)
            {
                UpdateFOVForSprint(false);
            }
        }
        
        // Update the tracking variable for next frame
        _wasSprintingLastFrame = isCurrentlySprinting && isMoving;
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void HandleCameraZoom()
    {
        if (_cinemachineFollowComponent != null)
        {
            // Read scroll wheel input
            float scrollValue = _playerControls.Player.Scroll.ReadValue<float>();
            
            if (Mathf.Abs(scrollValue) > 0.01f)
            {
                // Calculate new camera distance
                _currentCameraDistance -= scrollValue * ZoomSpeed * Time.deltaTime * 10f;
                _currentCameraDistance = Mathf.Clamp(_currentCameraDistance, MinCameraDistance, MaxCameraDistance);
                
                // Apply to cinemachine
                _cinemachineFollowComponent.CameraDistance = _currentCameraDistance;
            }
        }
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z);
        _grounded = Physics.CheckSphere(spherePosition, 0.2f, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore);

        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, _grounded);
        }
    }

    private void CameraRotation()
    {
        // Only update camera rotation if right mouse button is pressed AND camera control is enabled
        if (_isRightMousePressed && EnableCameraControl)
        {
            Vector2 lookInput = _playerControls.Player.Look.ReadValue<Vector2>();
            
            if (lookInput.sqrMagnitude >= _threshold)
            {
                // Scale input by sensitivity
                float sensitivity = 1.0f;
                _cinemachineTargetYaw += lookInput.x * sensitivity;
                _cinemachineTargetPitch -= lookInput.y * sensitivity;
            }

            // Clamp pitch
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
        }

        // Always apply rotation to camera target - this ensures the camera follows even when not controlling it
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0.0f);
    }

    private void Move()
    {
        // Read input values
        Vector2 moveInput = _playerControls.Player.Move.ReadValue<Vector2>();
        //Debug.Log("moveInput: "+moveInput);
        bool isSprinting = _playerControls.Player.Sprint.IsPressed();
        
        // Set target speed based on movement mode
        float targetSpeed = isSprinting ? SprintSpeed : MoveSpeed;

        // If no input, set target speed to 0
        if (moveInput == Vector2.zero) targetSpeed = 0.0f;

        // Get current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = moveInput.magnitude;

        // Accelerate or decelerate smoothly
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * 10.0f);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }
        
        // Normalize input direction
        Vector3 inputDirection = new Vector3(moveInput.x, 0.0f, moveInput.y).normalized;

        // Only update rotation if we're actually moving
        if (moveInput != Vector2.zero)
        {
            // Always calculate target rotation based on camera orientation
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + 
                            _mainCamera.transform.eulerAngles.y;
            
            // Smoothly rotate towards the target rotation
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, 
                                                ref _rotationVelocity, RotationSmoothTime);

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        // Move in the direction we're facing
        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        // Move the player
        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + 
                        new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        // Update animator with single "Speed" parameter for simple Idle/Walk states
        if (_hasAnimator)
        {
            _animator.SetFloat("Speed", _speed);
        }
    }

    private void JumpAndGravity()
    {
        if (_grounded)
        {
            // Reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // Update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }

            // Stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (_playerControls.Player.Jump.IsPressed() && _jumpTimeoutDelta <= 0.0f)
            {
                // The square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                // Update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, true);
                }
            }

            // Jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // Reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // Fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // Update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDFreeFall, true);
                }
            }
        }

        // Apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}