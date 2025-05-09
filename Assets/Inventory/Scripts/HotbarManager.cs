using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarManager : MonoBehaviour
{
    [Header("Hotbar Configuration")]
    [SerializeField] private List<HotbarSlot> hotbarSlots = new List<HotbarSlot>();
    
    [Header("Runtime Data")]
    [SerializeField] private int selectedSlotIndex = -1;
    [SerializeField] private bool showDebugLogs = false;
    
    // Reference to inventory manager
    private InventoryManager inventoryManager;
    private PlayerControls controls;

    private bool ignoreInput;
    
    private void Start()
    {
        inventoryManager = InventoryManager.Instance;
        if (inventoryManager == null)
        {
            Debug.LogError("HotbarManager: InventoryManager not found!");
            return;
        }
        
        // Initialize slots with correct bookmark indices
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            if (hotbarSlots[i] != null)
            {
                hotbarSlots[i].Initialize(i + 1); // Set indices to 1-5
            }
        }
        controls = new PlayerControls();
        controls.UI.HotbarSlots.Enable();
        controls.UI.HotbarSlots.performed += OnHotbarKeyPressed;
        PlayerEvents.OnPlayerEnterMinigame += HandleEnterMinigame;
        
        // Subscribe to inventory changes
        inventoryManager.OnInventoryChanged += RefreshHotbar;
        
        // Do initial refresh
        RefreshHotbar();
    }

    private void HandleEnterMinigame(bool inMinigame)
    {
        ignoreInput = inMinigame;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryChanged -= RefreshHotbar;
        }
        controls.UI.HotbarSlots.performed -= OnHotbarKeyPressed;
        controls.UI.HotbarSlots.Disable();
    }
    
    // Refresh all hotbar slots based on bookmarked items
    public void RefreshHotbar()
    {
        if (inventoryManager == null)
            return;
            
        // Update each hotbar slot
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            if (hotbarSlots[i] != null)
            {
                int bookmarkIndex = i + 1; // Converts 0-based index to 1-5
                
                // Get item with this bookmark from inventory manager
                InventoryItem bookmarkedItem = inventoryManager.GetItemByBookmark(bookmarkIndex);
                
                // Set the slot's item
                hotbarSlots[i].SetItem(bookmarkedItem);
            }
        }
        
        // Make sure selected slot is still valid
        ValidateSelectedSlot();
    }
    
    // Called by Player Input Component when hotkeys 1-5 are pressed
    public void OnHotbarKeyPressed(InputAction.CallbackContext context)
    {
        if (ignoreInput)
            return;
            
        // The value from number keys 1-5 will be 1.0f to 5.0f
        float inputValue = context.ReadValue<float>();
        
        // Convert to int (1-5)
        int hotkeyNumber = Mathf.RoundToInt(inputValue);
        
        if (showDebugLogs) Debug.Log($"Hotbar hotkey pressed: {hotkeyNumber}");
        
        // Convert 1-based hotkey number to 0-based slot index
        SelectSlot(hotkeyNumber - 1);
    }
    
    // Select a hotbar slot by index (0-based)
    public void SelectSlot(int index)
    {
        if (index < 0 || index >= hotbarSlots.Count)
            return;
            
        // Deselect current slot if there is one
        if (selectedSlotIndex >= 0 && selectedSlotIndex < hotbarSlots.Count)
        {
            hotbarSlots[selectedSlotIndex].SetClickedState(false);
        }
        
        // If selecting the same slot, toggle it off
        if (selectedSlotIndex == index)
        {
            if (showDebugLogs) Debug.Log("Unequipping item from hotbar");
            selectedSlotIndex = -1; // No selection
            
            // Report that nothing is equipped
            ReportToInventoryManager(null);
        }
        else
        {
            // Select the new slot
            selectedSlotIndex = index;
            
            // Only set as clicked if the slot has an item
            if (!hotbarSlots[selectedSlotIndex].IsEmpty)
            {
                hotbarSlots[selectedSlotIndex].SetClickedState(true);
                
                // Report the equipped item to inventory manager
                InventoryItem equippedItem = hotbarSlots[selectedSlotIndex].Item;
                if (showDebugLogs && equippedItem != null)
                    Debug.Log($"Equipped item: {equippedItem.ItemData.name}");
                    
                ReportToInventoryManager(equippedItem);
            }
            else
            {
                // Slot is empty, don't select it
                selectedSlotIndex = -1;
                
                // Report that nothing is equipped
                ReportToInventoryManager(null);
            }
        }
    }
    
    // Report equipped item to inventory manager
    private void ReportToInventoryManager(InventoryItem item)
    {
        if (inventoryManager != null)
        {
            inventoryManager.SetEquippedItem(item);
        }
    }
    
    // Get the currently selected/equipped item (if any)
    public InventoryItem GetSelectedItem()
    {
        if (selectedSlotIndex >= 0 && selectedSlotIndex < hotbarSlots.Count)
        {
            return hotbarSlots[selectedSlotIndex].Item;
        }
        
        return null;
    }
    
    // Ensure the selected slot is still valid (e.g., if item was removed)
    private void ValidateSelectedSlot()
    {
        if (selectedSlotIndex >= 0 && selectedSlotIndex < hotbarSlots.Count)
        {
            if (hotbarSlots[selectedSlotIndex].IsEmpty)
            {
                // Deselect if slot is now empty
                SelectSlot(-1);
            }
        }
    }
}