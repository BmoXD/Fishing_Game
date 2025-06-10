using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class InteractionPoint : MonoBehaviour
{
    public GameObject uiPrefab; // Prefab to instantiate as UI element
    public Vector2 elementSize = new Vector2(50, 50); // Optional: override size

    private Canvas uiCanvas;
    private RectTransform uiElementRect;
    private GameObject uiElementInstance;
    private IInteractable interactable;
    private bool isPlayerNear = false;
    private bool isMenuOpen = false;

    void Start()
    {
        // Find the first Canvas in the scene if not assigned
        uiCanvas = FindObjectOfType<Canvas>();
        if (uiCanvas == null)
        {
            Debug.LogError("No Canvas found in the scene. Please add a Canvas.");
            enabled = false;
            return;
        }

        if (uiPrefab == null)
        {
            Debug.LogError("No UI prefab assigned to WorldToUISprite.");
            enabled = false;
            return;
        }

        interactable = GetComponent<IInteractable>();
        if (interactable == null)
        {
            Debug.LogWarning("No IInteractable found on " + gameObject.name);
        }
    }

    void OnEnable()
    {
        CinemachineCore.CameraUpdatedEvent.AddListener(OnCameraUpdated);
        PlayerEvents.OnPlayerEnterMenu += HandleEnterMenu;
    }

    void OnDisable()
    {
        CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCameraUpdated);
    }

    private void HandleEnterMenu(bool inMenu)
    {
        isMenuOpen = inMenu;

        // Hide the UI element if menu is open
        if (uiElementInstance != null)
            uiElementInstance.SetActive(!isMenuOpen);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;

            // Instantiate the UI element if it doesn't exist
            if (uiElementInstance == null && uiPrefab != null && uiCanvas != null)
            {
                uiElementInstance = Instantiate(uiPrefab, uiCanvas.transform);
                uiElementRect = uiElementInstance.GetComponent<RectTransform>();
                if (uiElementRect != null)
                {
                    uiElementRect.sizeDelta = elementSize;
                }
                // Move to top of UI hierarchy
                uiElementInstance.transform.SetAsLastSibling();
            }

            if (uiElementInstance != null) uiElementInstance.SetActive(true);

            // Register with player
            other.GetComponent<ThirdPersonController>()?.SetActiveInteractionPoint(this);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;

            // Destroy the UI element when the player leaves
            if (uiElementInstance != null)
            {
                Destroy(uiElementInstance);
                uiElementInstance = null;
                uiElementRect = null;
            }

            // Unregister with player
            other.GetComponent<ThirdPersonController>()?.SetActiveInteractionPoint(null);
        }
    }

    // Call this when the player presses the use key and is in range
    public void TryInteract()
    {
        if (interactable != null && isPlayerNear && !isMenuOpen)
        {
            interactable.Interact();
        }
    }

    void OnCameraUpdated(CinemachineBrain brain)
    {
        if (Camera.main == null) return;
        // Convert world position to screen point
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

        // If behind camera or menu is open, hide the element
        if (screenPos.z < 0 || isMenuOpen)
        {
            if (uiElementInstance != null) uiElementInstance.SetActive(false);
        }
        else
        {
            if (uiElementInstance != null)
            {
                uiElementInstance.SetActive(true);
                uiElementInstance.transform.SetAsFirstSibling();
                uiElementRect.position = screenPos;
            }
        }
    }
}