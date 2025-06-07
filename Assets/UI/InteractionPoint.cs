using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class InteractionPoint : MonoBehaviour, IInteractable
{
    public GameObject uiPrefab; // Prefab to instantiate as UI element
    public Vector2 elementSize = new Vector2(50, 50); // Optional: override size

    // Optional: assign a target for interaction (e.g. NPC script)
    public MonoBehaviour interactionTarget;

    private Canvas uiCanvas;
    private RectTransform uiElementRect;
    private GameObject uiElementInstance;

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

        // Instantiate the prefab as a child of the canvas
        uiElementInstance = Instantiate(uiPrefab, uiCanvas.transform);
        uiElementRect = uiElementInstance.GetComponent<RectTransform>();
        if (uiElementRect != null)
        {
            uiElementRect.sizeDelta = elementSize;
        }
    }

    void OnEnable()
    {
        CinemachineCore.CameraUpdatedEvent.AddListener(OnCameraUpdated);
    }

    void OnDisable()
    {
        CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCameraUpdated);
    }

    public void Interact(GameObject interactor)
    {
        // If a target is set and implements IInteractable, delegate to it
        if (interactionTarget is IInteractable target && target != this)
        {
            target.Interact(interactor);
        }
        else
        {
            // Fallback: interact with self
            Debug.Log($"{gameObject.name} was interacted with by {interactor.name}");
        }
    }

    void OnCameraUpdated(CinemachineBrain brain)
    {
        if (Camera.main == null) return;
        // Convert world position to screen point
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

        // If behind camera, hide the element
        if (screenPos.z < 0)
        {
            if (uiElementInstance != null) uiElementInstance.SetActive(false);
        }
        else
        {
            if (uiElementInstance != null)
            {
                uiElementInstance.SetActive(true);
                uiElementRect.position = screenPos;
            }
        }
    }
}