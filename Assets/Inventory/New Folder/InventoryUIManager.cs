using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private GameObject slotPrefab;
    
    [Header("Runtime Data")]
    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();
    [SerializeField] private InventorySlot hoveredSlot;
    [SerializeField] private InventorySlot selectedSlot;

    private PlayerControls controls;
    
    // Reference to inventory manager
    private InventoryManager inventoryManager;
    
    private void Awake()
    {
        // Get the inventory manager
        // inventoryManager = InventoryManager.Instance;
        // if (inventoryManager == null)
        // {
        //     Debug.LogError("InventoryUIManager: InventoryManager not found!");
        // }
    }
    
    private void Start()
    {
        inventoryManager = InventoryManager.Instance;
        if (inventoryManager == null)
        {
            Debug.LogError("InventoryUIManager: InventoryManager not found!");
        }

        // Subscribe to the event here to ensure we have a valid reference
        inventoryManager.OnInventoryChanged += RefreshInventory;
    

        RefreshInventory();
    }
    
    private void OnEnable()
    {
        // Subscribe to inventory events
        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryChanged += RefreshInventory;
        }

        controls = new PlayerControls();
        controls.UI.HotbarSlots.Enable();
        controls.UI.HotbarSlots.performed += OnHotbarSlots;
        
        //_1 = _playerInput.actions["HotbarSlots"].bindings[1];
    }

    public void OnHotbarSlots(InputAction.CallbackContext context)
    {
        int inputValue = (int)context.ReadValue<float>();
        Debug.Log(inputValue);

        if (hoveredSlot == null || hoveredSlot.Item == null)
            return;

        InventoryItem itemToBookmark = hoveredSlot.Item;

        // If item is already bookmarked to this slot, unbookmark it
        if (itemToBookmark.BookmarkSlot == inputValue)
        {
            inventoryManager.SetBookmark(itemToBookmark.ItemData, 0); // 0 means no bookmark
        }
        else
        {
            // Otherwise bookmark it to the selected slot
            inventoryManager.SetBookmark(itemToBookmark.ItemData, inputValue);
        }
    }
    
    private void OnDisable()
    {
        // Unsubscribe from inventory events
        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryChanged -= RefreshInventory;
        }
        controls.UI.HotbarSlots.Disable();
        controls.UI.HotbarSlots.performed -= OnHotbarSlots;
        
        // Clear selection
        hoveredSlot = null;
        selectedSlot = null;
    }

    // // Called by the Player Input component when HotbarSlots action is triggered
    // public void OnHotbarSlots(InputValue value)
    // {
    //     Debug.Log(value);
    //     // Only process if we're hovering over a slot with an item
    //     if (hoveredSlot == null || hoveredSlot.Item == null)
    //         return;
            
    //     // The value from number keys 1-5 will be in the range of 1.0f to 5.0f
    //     float inputValue = value.Get<float>();
        
    //     // Convert to int (1-5)
    //     int bookmarkSlot = Mathf.RoundToInt(inputValue);
        
    //     // Make sure it's in valid range
    //     if (bookmarkSlot < 1 || bookmarkSlot > 5)
    //         return;
            
    //     // Get the item to bookmark
    //     InventoryItem itemToBookmark = hoveredSlot.Item;
        
    //     // If item is already bookmarked to this slot, unbookmark it
    //     if (itemToBookmark.BookmarkSlot == bookmarkSlot)
    //     {
    //         inventoryManager.SetBookmark(itemToBookmark.ItemData, 0); // 0 means no bookmark
    //     }
    //     else
    //     {
    //         // Otherwise bookmark it to the selected slot
    //         inventoryManager.SetBookmark(itemToBookmark.ItemData, bookmarkSlot);
    //     }
        
    //     // No need to refresh UI manually, as the InventoryManager will trigger the OnInventoryChanged event
    // }
    
    // Refresh the inventory display
    public void RefreshInventory()
    {
        if (inventoryManager == null || slotsContainer == null || slotPrefab == null)
            return;
        
        // Get all inventory items
        List<InventoryItem> items = inventoryManager.GetAllItems();
        
        // Create/update slots as needed
        EnsureSlotsCount(items.Count);
        
        // Assign items to slots
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < items.Count)
            {
                slots[i].SetItem(items[i]);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
        
        // Remove excess slots
        RemoveExcessSlots(items.Count);
    }

    // Remove slots that exceed the required count
    private void RemoveExcessSlots(int requiredCount)
    {
        if (slots.Count <= requiredCount)
            return;
            
        // Remove slots from the end of the list
        while (slots.Count > requiredCount)
        {
            int lastIndex = slots.Count - 1;
            InventorySlot slotToRemove = slots[lastIndex];
            
            // If this is the selected or hovered slot, clear those references
            if (selectedSlot == slotToRemove)
                selectedSlot = null;
                
            if (hoveredSlot == slotToRemove)
                hoveredSlot = null;
                
            // Destroy the GameObject and remove from our list
            Destroy(slotToRemove.gameObject);
            slots.RemoveAt(lastIndex);
        }
    }
    
    // Make sure we have enough slots
    private void EnsureSlotsCount(int count)
    {
        // Create new slots if needed
        while (slots.Count < count)
        {
            CreateSlot();
        }
    }
    
    // Create a new slot
    private InventorySlot CreateSlot()
    {
        GameObject slotObj = Instantiate(slotPrefab, slotsContainer);
        InventorySlot slot = slotObj.GetComponent<InventorySlot>();
        
        if (slot != null)
        {
            slot.Initialize(slots.Count);
            slots.Add(slot);
        }
        
        return slot;
    }
    
    // Called when a slot is hovered
    public void OnSlotHovered(InventorySlot slot)
    {
        hoveredSlot = slot;
        
        // You can add additional functionality here, like showing a tooltip
    }
    
    // Called when a slot is unhovered
    public void OnSlotUnhovered(InventorySlot slot)
    {
        if (hoveredSlot == slot)
        {
            hoveredSlot = null;
            
            // You can add additional functionality here
        }
    }
    
    // Called when a slot is clicked
    public void OnSlotClicked(InventorySlot slot)
    {
        // Deselect previous slot if different
        if (selectedSlot != null && selectedSlot != slot)
        {
            selectedSlot.SetClickedState(false);
        }
        
        // Toggle selection
        if (selectedSlot == slot)
        {
            slot.SetClickedState(!slot.IsClicked());
            selectedSlot = slot.IsClicked() ? slot : null;
        }
        else
        {
            slot.SetClickedState(true);
            selectedSlot = slot;
        }
        
        // You can add additional click functionality here
    }
    
    // Get the currently selected item (if any)
    public InventoryItem GetSelectedItem()
    {
        return selectedSlot != null ? selectedSlot.Item : null;
    }
}