using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class FishingRod : ItemFunctionality
{
    private Transform itemSocket;
    private ParentConstraint parentConstraint;
    private Animator playerAnimator;

    [Header("Charging/Aiming")]
    [SerializeField] private GameObject aimIndicatorPrefab; // Assign a prefab for the plane
    [SerializeField] private float aimStartHeight = 2f;
    [SerializeField] private float aimStartForward = 1f;
    [SerializeField] private float aimMoveSpeed = 6f;
    [SerializeField] private float aimMaxDistance = 15f;

    [SerializeField] private Transform rodTip; // Assign in inspector: the tip of the rod
    [SerializeField] private GameObject bobberPrefab; // Assign your bobber prefab in inspector
    [SerializeField] private GameObject fishingLinePrefab; // Assign a prefab with CurvedLine

    private GameObject aimObject;
    private GameObject aimPlane;
    private Transform playerTransform;
    private bool isCharging = false;
    private float currentAimDistance = 0f;
    private bool isFishing = false;
    private RaycastHit lastAimHit;

    private GameObject spawnedBobber;
    private CurvedLine spawnedLine;

    private bool isWaitingForBite = false;
    private float biteTimer = 0f;
    private float biteTimerMax = 0f;
    private bool minigameActive = false;
    private Item caughtItem;
    private float caughtWeight = 0f;
    private WaterVolume currentWaterVolume;
    private UIManager uiManager;

    public override void Use()
    {
        Debug.Log("Fishing rod used");
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        Debug.Log("Fishing Rod OnEnable");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player object not found!");
            return;
        }

        // Get the player's Animator
        playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator == null)
        {
            Debug.LogError("Player Animator not found!");
        }

        playerTransform = player.transform;

        GameObject socketObject = GameObject.FindGameObjectWithTag("Socket");
        if (socketObject == null)
        {
            Debug.LogError("Socket object not found!");
            return;
        }

        if (!socketObject.transform.IsChildOf(player.transform))
        {
            Debug.LogError("Socket is not a child of the player!");
            return;
        }

        uiManager = UIManager.Instance;
        if (uiManager != null)
        {
            var minigame = uiManager.GetComponentInChildren<FishingMinigame>(true);
            if (minigame != null)
            {
                minigame.onMinigameSuccess.AddListener(OnMinigameSuccess);
                minigame.onMinigameFail.AddListener(OnMinigameFail);
            }
        }

        itemSocket = socketObject.transform;

        parentConstraint = gameObject.AddComponent<ParentConstraint>();

        ConstraintSource source = new ConstraintSource
        {
            sourceTransform = itemSocket,
            weight = 1.0f
        };
        parentConstraint.AddSource(source);

        parentConstraint.constraintActive = true;
        parentConstraint.locked = true;

        Debug.Log("Fishing rod attached to ItemSocket");

        //playerController = player.GetComponent<ThirdPersonController>();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        Debug.Log("Fishing Rod OnDisable");
        if (parentConstraint != null)
        {
            Destroy(parentConstraint);
            parentConstraint = null;
        }
        // if (controls != null)
        // {
        //     controls.Item.Use.started -= OnUseStarted;
        //     controls.Item.Use.canceled -= OnUseCanceled;
        //     controls.Item.Disable();
        //     controls = null;
        // }
        CleanupAimObjects();

        // Cleanup spawned bobber and line
        if (spawnedBobber != null)
        {
            Destroy(spawnedBobber);
            spawnedBobber = null;
        }
        if (spawnedLine != null)
        {
            Destroy(spawnedLine.gameObject);
            spawnedLine = null;
        }
        if (uiManager != null)
        {
            var minigame = uiManager.GetComponentInChildren<FishingMinigame>(true);
            if (minigame != null)
            {
                minigame.onMinigameSuccess.RemoveListener(OnMinigameSuccess);
                minigame.onMinigameFail.RemoveListener(OnMinigameFail);
            }
        }
        PlayerEvents.RaiseFishingStateChanged(false);
    }

    private void Update()
    {
        if (isFishing && isWaitingForBite && !minigameActive)
        {
            biteTimer += Time.deltaTime;
            if (biteTimer >= biteTimerMax)
            {
                isWaitingForBite = false;
                // Fish bites!
                Debug.Log("A fish is on the hook! Press Use to start the minigame.");
                // Optionally, play a bobber animation or sound here
            }
        }

        if (isCharging && aimObject != null && playerTransform != null)
        {
            // Move aimObject forward until max distance
            if (currentAimDistance < aimMaxDistance)
            {
                float moveStep = aimMoveSpeed * Time.deltaTime;
                currentAimDistance = Mathf.Min(currentAimDistance + moveStep, aimMaxDistance);
                aimObject.transform.position = playerTransform.position
                    + Vector3.up * aimStartHeight
                    + playerTransform.forward * (aimStartForward + currentAimDistance);
            }

            // Raycast down to ground
            Ray ray = new Ray(aimObject.transform.position, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                lastAimHit = hit;
                if (aimPlane == null && aimIndicatorPrefab != null)
                {
                    aimPlane = Instantiate(aimIndicatorPrefab, hit.point, Quaternion.identity);
                }
                else if (aimPlane != null)
                {
                    aimPlane.transform.position = hit.point;
                }
            }
        }
    }

    protected override void OnUseStarted(InputAction.CallbackContext context)
    {
        base.OnUseStarted(context);
        if (IsPointerOverUI() || isMenuOpen)
            return;
        if (minigameActive)
            return;
            
        if (isFishing)
        {
            if (!isWaitingForBite && !minigameActive)
            {
                // Start minigame
                minigameActive = true;
                // Pick a random item from the water
                if (currentWaterVolume != null)
                {
                    var catchResult = currentWaterVolume.GetRandomItem();
                    caughtItem = catchResult.item;
                    caughtWeight = catchResult.weight;
                }
                else
                {
                    caughtItem = null;
                    caughtWeight = 0f;
                }

                // Open the minigame panel with desired difficulty
                if (uiManager != null)
                {
                    // You can pass custom values or use fields
                    uiManager.OpenMinigamePanel(leftDriftIntensity: 1.2f + (caughtWeight / 1000f) * 2f, rightPushIntensity: 0.5f + (caughtWeight / 1000f) * 3f);
                }
                return;
            }
            // Stop fishing
            if (playerAnimator != null)
            {
                playerAnimator.SetBool("Fishing", false);
                playerAnimator.SetTrigger("CastRod");
            }
            isFishing = false;
            Debug.Log("Stopped fishing.");

            // Animate bobber back to rod tip, then clean up
            if (spawnedBobber != null && rodTip != null)
            {
                StartCoroutine(AnimateBobberBackAndCleanup(spawnedBobber.transform, spawnedBobber.transform.position, rodTip.position, 1.0f));
            }
            PlayerEvents.RaiseFishingStateChanged(false);
            return;
        }

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("ChargingRod", true);
            playerAnimator.SetTrigger("CastRod");
        }

        // Spawn aim object above and in front of player
        if (aimObject == null && playerTransform != null)
        {
            Debug.Log("Spawning Aim Object");
            aimObject = new GameObject("RodAimObject");
            aimObject.transform.position = playerTransform.position
                + Vector3.up * aimStartHeight
                + playerTransform.forward * aimStartForward;
            aimObject.transform.parent = playerTransform; // Constrain to player
            currentAimDistance = 0f;
        }
        isCharging = true;
    }

    protected override void OnUseCanceled(InputAction.CallbackContext context)
    {
        base.OnUseCanceled(context);
        if (IsPointerOverUI() || isMenuOpen)
            return;
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("ChargingRod", false);
            playerAnimator.SetTrigger("CastRod");
        }
        isCharging = false;

        // Check if last raycast hit water
        if (lastAimHit.collider != null && lastAimHit.collider.CompareTag("Water"))
        {
            Debug.Log("Bobber hit water! Start fishing. "+ lastAimHit.point);
            if (playerAnimator != null)
                playerAnimator.SetBool("Fishing", true);
            isFishing = true;
            PlayerEvents.RaiseFishingStateChanged(true);

            // Spawn and animate bobber
            if (bobberPrefab != null && rodTip != null)
            {
                if (spawnedBobber != null) Destroy(spawnedBobber);
                spawnedBobber = Instantiate(bobberPrefab, rodTip.position, Quaternion.identity);

                // Optionally, destroy previous line
                if (spawnedLine != null) Destroy(spawnedLine.gameObject);

                // Spawn line and set points
                if (fishingLinePrefab != null)
                {
                    GameObject lineObj = Instantiate(fishingLinePrefab);
                    spawnedLine = lineObj.GetComponent<CurvedLine>();
                    spawnedLine.SetStartPoint(rodTip);
                    spawnedLine.SetEndPoint(spawnedBobber.transform);
                }

                // Animate bobber to hit point
                StartCoroutine(AnimateBobberToTarget(spawnedBobber.transform, rodTip.position, lastAimHit.point, 1.0f));
            }
            // Find WaterVolume at hit point
            currentWaterVolume = lastAimHit.collider.GetComponent<WaterVolume>();

            // Start bite timer
            biteTimerMax = Random.Range(2f, 5f); // Random wait 2-5 seconds
            biteTimer = 0f;
            isWaitingForBite = true;

        }
    
        CleanupAimObjects();

        if (isFishing == false)
            PlayerEvents.RaiseFishingStateChanged(false);
    }

    // Coroutine to animate bobber in an arc
    private System.Collections.IEnumerator AnimateBobberToTarget(Transform bobber, Vector3 start, Vector3 end, float duration)
    {
        // Debug.Log("Animating");
        float elapsed = 0f;
        Vector3 mid = (start + end) / 2f + Vector3.up * 3f; // Adjust height for arc

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // Quadratic Bezier curve
            Vector3 pos = Mathf.Pow(1 - t, 2) * start +
                        2 * (1 - t) * t * mid +
                        Mathf.Pow(t, 2) * end;
            bobber.position = pos;
            // Debug.Log("pos: "+pos);
            // Debug.Log("bobber.position: "+bobber.position);
            elapsed += Time.deltaTime;
            yield return null;
        }
        bobber.position = end;

        // Play splash particle effect at landing
        var splash = bobber.GetComponentInChildren<ParticleSystem>();
        if (splash != null)
            splash.Play();
    }

    // Coroutine to animate bobber back and clean up
    private System.Collections.IEnumerator AnimateBobberBackAndCleanup(Transform bobber, Vector3 start, Vector3 end, float duration)
    {
        // Play splash particle effect at pull-out (before moving)
        var splash = bobber.GetComponentInChildren<ParticleSystem>();
        if (splash != null)
            splash.Play();

        float elapsed = 0f;
        Vector3 mid = (start + end) / 2f + Vector3.up * 3f; // Adjust height for arc

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // Quadratic Bezier curve (reverse)
            Vector3 pos = Mathf.Pow(1 - t, 2) * start +
                        2 * (1 - t) * t * mid +
                        Mathf.Pow(t, 2) * end;
            bobber.position = pos;
            elapsed += Time.deltaTime;
            yield return null;
        }
        bobber.position = end;

        // Cleanup bobber and line
        if (spawnedBobber != null)
        {
            Destroy(spawnedBobber);
            spawnedBobber = null;
        }
        if (spawnedLine != null)
        {
            Destroy(spawnedLine.gameObject);
            spawnedLine = null;
        }
    }

    private void OnMinigameSuccess()
    {
        minigameActive = false;

        if (caughtItem != null)
        {
            InventoryItem invItem = InventoryManager.Instance.AddItemAsNewInstance(caughtItem);
            if (invItem != null)
            {
                invItem.Weight = caughtWeight;
            }
            Debug.Log($"You caught: {caughtItem.itemName} ({caughtWeight:0.##}g)");
        }
        else
        {
            Debug.Log("You caught: Nothing");
        }

        CleanupFishing();
    }

    private void OnMinigameFail()
    {
        minigameActive = false;
        Debug.Log("The fish got away!");
        CleanupFishing();
    }

    private void CleanupFishing()
    {
        // Reset fishing state, clean up bobber, etc.
        isFishing = false;
        isWaitingForBite = false;
        biteTimer = 0f;
        biteTimerMax = 0f;
        caughtItem = null;
        currentWaterVolume = null;

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("Fishing", false);
            playerAnimator.SetBool("ChargingRod", false);
            playerAnimator.SetTrigger("CastRod");
        }


        // Animate bobber back to rod tip, then clean up
        if (spawnedBobber != null && rodTip != null)
        {
            StartCoroutine(AnimateBobberBackAndCleanup(spawnedBobber.transform, spawnedBobber.transform.position, rodTip.position, 1.0f));
        }

        PlayerEvents.RaiseFishingStateChanged(false);
    }

    private void CleanupAimObjects()
    {
        if (aimObject != null)
        {
            Destroy(aimObject);
            aimObject = null;
        }
        if (aimPlane != null)
        {
            Destroy(aimPlane);
            aimPlane = null;
        }
        lastAimHit = new RaycastHit();
    }
}