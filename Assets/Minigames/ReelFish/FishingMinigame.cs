using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class FishingMinigame : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform barRectTransform;
    [SerializeField] private RectTransform cursorRectTransform;
    
    [Header("Gameplay Settings")]
    [Range(0.1f, 10f)]
    [SerializeField] public float leftDriftIntensity = 1.2f;
    [Range(0.1f, 10f)]
    [SerializeField] public float rightPushIntensity = 0.5f;
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float movementSmoothTime = 0.3f;
    
    [Header("Progress Settings")]
    [SerializeField] private float progressPerSecond = 10f;
    [SerializeField] private float progressPerClick = 20f;
    [SerializeField] private float maxProgress = 100f;
    [SerializeField] private bool autoActivateForDebug = true;
    
    [Header("Zone Settings")]
    [Range(0f, 0.5f)]
    [SerializeField] private float leftRedZone = 0.3f;
    [Range(0.5f, 1f)]
    [SerializeField] private float rightRedZone = 0.7f;
    [SerializeField] private bool showDebugZones = true;
    [SerializeField] private Color debugZoneColor = Color.yellow;
    [SerializeField] private float debugLineThickness = 5f;
    
    [Header("Events")]
    public UnityEvent onMinigameStart;
    public UnityEvent onMinigameSuccess;
    public UnityEvent onMinigameFail;
    
    // Internal variables
    private bool isActive = false;
    private float currentPosition = 0.5f; // 0 = right, 1 = left, 0.5 = middle
    private float targetPosition = 0.5f;
    private float currentProgress = 0f;
    private float positionVelocity = 0f;
    private RectTransform leftZoneVisual;
    private RectTransform rightZoneVisual;
    
    // Input System variables
    // private PlayerInput playerInput;
    // private InputAction clickAction;

    private PlayerControls controls;
    
    private void Awake()
    {
        // // Get PlayerInput component
        // playerInput = GetComponent<PlayerInput>();
        
        // // Get the click action reference
        // clickAction = playerInput.actions["LeftMouse"];
    }
    
    private void OnEnable()
    {
        // Subscribe to the click action event
        controls = new PlayerControls();
        controls.UI.Click.Enable();
        controls.UI.Click.performed += OnClick;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from the click action event
        controls.UI.Click.performed -= OnClick;
        controls.UI.Click.Disable();
    }
    
    private void Start()
    {
        // Set up debug visuals if enabled
        if (showDebugZones)
        {
            CreateDebugVisuals();
        }
        
        // Initialize cursor position
        UpdateCursorPosition();
        
        // Auto-activate if debug mode is on
        if (autoActivateForDebug)
        {
            Activate();
        }
    }
    
    private void OnValidate()
    {
        // Update debug visuals when values change in editor
        if (Application.isEditor && showDebugZones && isActiveAndEnabled)
        {
            if (leftZoneVisual != null && rightZoneVisual != null)
            {
                UpdateDebugVisuals();
            }
            else if (gameObject.activeInHierarchy)
            {
                CreateDebugVisuals();
            }
        }
    }
    
    private void Update()
    {
        if (!isActive) return;
        
        // Natural drift to the left
        targetPosition += Time.deltaTime * leftDriftIntensity * 0.1f;
        targetPosition = Mathf.Clamp01(targetPosition);
        
        // Smooth movement
        currentPosition = Mathf.SmoothDamp(currentPosition, targetPosition, ref positionVelocity, movementSmoothTime);
        
        // Update visual position
        UpdateCursorPosition();
        
        // Check for failure conditions
        if (currentPosition <= leftRedZone || currentPosition >= rightRedZone)
        {
            Fail();
            return;
        }
        
        // Update progress
        currentProgress += Time.deltaTime * progressPerSecond;
        if (currentProgress >= maxProgress)
        {
            Succeed();
        }
    }
    
    // Input System callback for click action
    private void OnClick(InputAction.CallbackContext context)
    {
        if (isActive)
        {
            PushRight();
        }
    }
    
    private void UpdateCursorPosition()
    {
        if (cursorRectTransform != null && barRectTransform != null)
        {
            // Map position (0-1) to actual UI position
            float width = barRectTransform.rect.width;
            float xPos = Mathf.Lerp(width * 0.5f, -width * 0.5f, currentPosition);
            
            cursorRectTransform.anchoredPosition = new Vector2(xPos, cursorRectTransform.anchoredPosition.y);
        }
    }
    
    private void UpdateDebugVisuals()
    {
        if (leftZoneVisual != null && rightZoneVisual != null && barRectTransform != null)
        {
            float width = barRectTransform.rect.width;
            
            // Position left zone marker
            float leftXPos = Mathf.Lerp(width * 0.5f, -width * 0.5f, leftRedZone);
            leftZoneVisual.anchoredPosition = new Vector2(leftXPos, 0);
            
            // Position right zone marker
            float rightXPos = Mathf.Lerp(width * 0.5f, -width * 0.5f, rightRedZone);
            rightZoneVisual.anchoredPosition = new Vector2(rightXPos, 0);
        }
    }
    
    private void CreateDebugVisuals()
    {
        if (barRectTransform == null) return;
        
        // Create left zone visual if it doesn't exist
        if (leftZoneVisual == null)
        {
            leftZoneVisual = CreateDebugLine("LeftZoneMarker");
        }
        
        // Create right zone visual if it doesn't exist
        if (rightZoneVisual == null)
        {
            rightZoneVisual = CreateDebugLine("RightZoneMarker");
        }
        
        // Update positions
        UpdateDebugVisuals();
    }
    
    private RectTransform CreateDebugLine(string name)
    {
        GameObject line = new GameObject(name);
        line.transform.SetParent(barRectTransform, false);
        
        RectTransform rectTransform = line.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0);
        rectTransform.anchorMax = new Vector2(0.5f, 1);
        rectTransform.sizeDelta = new Vector2(debugLineThickness, 0);
        
        Image image = line.AddComponent<Image>();
        image.color = debugZoneColor;
        
        return rectTransform;
    }
    
    public void Activate()
    {
        isActive = true;
        currentPosition = 0.5f;
        targetPosition = 0.5f;
        currentProgress = 0f;
        positionVelocity = 0f;
        
        UpdateCursorPosition();
        onMinigameStart?.Invoke();
        
        // Hide debug visuals during gameplay
        if (leftZoneVisual != null) leftZoneVisual.gameObject.SetActive(false);
        if (rightZoneVisual != null) rightZoneVisual.gameObject.SetActive(false);
    }
    
    private void PushRight()
    {
        // Push cursor to the right based on right push intensity
        float pushAmount = rightPushIntensity * 0.1f;
        float evaluatedPush = movementCurve.Evaluate(pushAmount);
        targetPosition -= evaluatedPush;
        targetPosition = Mathf.Clamp01(targetPosition);
        
        // Add progress for clicking
        currentProgress += progressPerClick;
    }
    
    private void Succeed()
    {
        isActive = false;
        onMinigameSuccess?.Invoke();
        Debug.Log("Fishing success! Fish caught!");
        
        // Show debug visuals again if in debug mode
        if (showDebugZones)
        {
            if (leftZoneVisual != null) leftZoneVisual.gameObject.SetActive(true);
            if (rightZoneVisual != null) rightZoneVisual.gameObject.SetActive(true);
        }
    }
    
    private void Fail()
    {
        isActive = false;
        onMinigameFail?.Invoke();
        Debug.Log("Fishing failed! Fish escaped!");
        
        // Show debug visuals again if in debug mode
        if (showDebugZones)
        {
            if (leftZoneVisual != null) leftZoneVisual.gameObject.SetActive(true);
            if (rightZoneVisual != null) rightZoneVisual.gameObject.SetActive(true);
        }
    }
    
    // Public methods to control the minigame externally
    public void StartMinigame()
    {
        Activate();
    }

    public void Configure(float leftDrift, float rightPush)
    {
        leftDriftIntensity = leftDrift;
        rightPushIntensity = rightPush;
    }
    
    public float GetCurrentProgress()
    {
        return (currentProgress / maxProgress) * 100f;
    }
}