using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : ItemSlot
{
    // Reference to the manager
    private InventoryUIManager uiManager;
    
    protected override void Awake()
    {
        base.Awake();

        Debug.Log("A");
        
        // Find the UI manager
        uiManager = GetComponentInParent<InventoryUIManager>();
        if (uiManager == null)
        {
            Debug.LogWarning("InventorySlot: No InventoryUIManager found in parent hierarchy");
        }
    }
    
    // Override the click event
    protected override void OnElementClicked()
    {
        // Notify the UI manager
        if (uiManager != null)
        {
            uiManager.OnSlotClicked(this);
        }
    }
    
    // Override pointer events to notify manager
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        
        // Notify the UI manager
        if (uiManager != null)
        {
            uiManager.OnSlotHovered(this);
        }
    }
    
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        
        // Notify the UI manager
        if (uiManager != null)
        {
            uiManager.OnSlotUnhovered(this);
        }
    }
}