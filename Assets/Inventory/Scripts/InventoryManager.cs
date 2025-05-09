using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    // Singleton pattern
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private List<InventoryItem> inventoryItems = new List<InventoryItem>();

    [SerializeField] private InventoryItem equippedItem;
    // Event for when equipped item changes
    public delegate void EquippedItemChangedHandler(InventoryItem newItem);
    public event EquippedItemChangedHandler OnEquippedItemChanged;

    // Event for inventory changes
    public delegate void InventoryChangedHandler();
    public event InventoryChangedHandler OnInventoryChanged;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Called by HotbarManager when equipped item changes
    public void SetEquippedItem(InventoryItem item)
    {
        // Only update if there's a change
        if (equippedItem != item)
        {
            equippedItem = item;
            
            // Remove any previously spawned equipped item
            GameObject[] oldEquippedObjects = GameObject.FindGameObjectsWithTag("EquippedItem");
            foreach (var obj in oldEquippedObjects)
            {
                Destroy(obj);
            }
            
            // Spawn the new equipped item if it exists
            if (equippedItem != null && equippedItem.ItemData.prefab != null)
            {
                // Spawn the item prefab
                GameObject spawnedItem = Instantiate(equippedItem.ItemData.prefab, 
                    transform.position, Quaternion.identity);
                
                // Tag it for easier management
                spawnedItem.tag = "EquippedItem";
                
                // If the item has functionality, attach it to the spawned prefab
                if (equippedItem.ItemData.functionalityScript != null)
                {
                    System.Type functionType = equippedItem.ItemData.functionalityScript.GetClass();
                    
                    if (functionType != null && typeof(ItemFunctionality).IsAssignableFrom(functionType))
                    {
                        // Add the functionality component to the spawned item
                        spawnedItem.AddComponent(functionType);
                    }
                }
            }
            
            // Notify listeners about the change
            OnEquippedItemChanged?.Invoke(equippedItem);
        }
    }

    // Add item to inventory (with unlimited slots)
    public bool AddItem(Item item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;

        // Check if item already exists
        InventoryItem existingItem = GetInventoryItem(item);
        
        if (existingItem != null)
        {
            existingItem.AddQuantity(quantity);
            OnInventoryChanged?.Invoke();
            return true;
        }
        else
        {
            // Add new item (no size limit)
            inventoryItems.Add(new InventoryItem(item, quantity));
            OnInventoryChanged?.Invoke();
            return true;
        }
    }

    // Add a new instance of an item instead of stacking
    public bool AddItemAsNewInstance(Item item)
    {
        if (item == null) return false;
        
        // Always create a new inventory item with quantity 1
        inventoryItems.Add(new InventoryItem(item, 1));
        OnInventoryChanged?.Invoke();
        return true;
    }

    // Remove a specific instance of an item
    public bool RemoveItemInstance(Item item)
    {
        if (item == null) return false;

        // Find all instances of this item
        List<InventoryItem> instances = inventoryItems.FindAll(i => i.ItemData.ItemID == item.ItemID);
        
        if (instances.Count > 0)
        {
            // Remove one instance (the last one found)
            inventoryItems.Remove(instances[instances.Count - 1]);
            OnInventoryChanged?.Invoke();
            return true;
        }
        
        return false;
    }

    // Count how many separate instances of an item exist
    public int GetItemInstanceCount(Item item)
    {
        if (item == null) return 0;
        
        return inventoryItems.FindAll(i => i.ItemData.ItemID == item.ItemID).Count;
    }

    // Remove item from inventory
    public bool RemoveItem(Item item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;

        InventoryItem existingItem = GetInventoryItem(item);
        
        if (existingItem != null)
        {
            existingItem.RemoveQuantity(quantity);
            
            if (existingItem.Quantity <= 0)
            {
                // Clear bookmark if item is removed
                if (existingItem.BookmarkSlot > 0)
                {
                    existingItem.BookmarkSlot = 0;
                }
                
                inventoryItems.Remove(existingItem);
            }
            
            OnInventoryChanged?.Invoke();
            return true;
        }
        
        return false;
    }

    // Get inventory item by Item
    public InventoryItem GetInventoryItem(Item item)
    {
        if (item == null) return null;
        return inventoryItems.Find(i => i.ItemData.ItemID == item.ItemID);
    }

    // Get inventory item by ID
    public InventoryItem GetInventoryItemById(int itemId)
    {
        return inventoryItems.Find(i => i.ItemData.ItemID == itemId);
    }

    // Check if item exists in inventory
    public bool HasItem(Item item, int quantity = 1)
    {
        InventoryItem inventoryItem = GetInventoryItem(item);
        return inventoryItem != null && inventoryItem.Quantity >= quantity;
    }

    // Get all inventory items
    public List<InventoryItem> GetAllItems()
    {
        return new List<InventoryItem>(inventoryItems);
    }

    // Bookmark management
    public void SetBookmark(Item item, int slot)
    {
        if (slot < 0 || slot > 5) return;

        // Clear existing item in this bookmark slot
        if (slot > 0)
        {
            foreach (var inventoryItem in inventoryItems)
            {
                if (inventoryItem.BookmarkSlot == slot)
                {
                    inventoryItem.BookmarkSlot = 0;
                }
            }
        }

        // Set bookmark for this item
        InventoryItem targetItem = GetInventoryItem(item);
        if (targetItem != null)
        {
            targetItem.BookmarkSlot = slot;
            OnInventoryChanged?.Invoke();
        }
    }

    // Get item by bookmark slot
    public InventoryItem GetItemByBookmark(int slot)
    {
        if (slot <= 0 || slot > 5) return null;
        return inventoryItems.Find(i => i.BookmarkSlot == slot);
    }

    // Use an item (basic implementation)
    public bool UseItem(Item item)
    {
        if (!HasItem(item)) return false;
        Debug.Log("Using item");

        // Create and use item functionality component
        if (item.functionalityScript != null)
        {
            System.Type functionType = item.functionalityScript.GetClass();
            
            if (functionType != null && typeof(ItemFunctionality).IsAssignableFrom(functionType))
            {
                GameObject tempObj = new GameObject("TempItemUse");
                ItemFunctionality functionality = tempObj.AddComponent(functionType) as ItemFunctionality;
                functionality.Use();
                Destroy(tempObj);
                
                // Remove consumable items
                if (item.DeleteOnConsume)
                {
                    RemoveItem(item, 1);
                }
                
                return true;
            }
        }
        
        return false;
    }

    // Set a custom weight for an item in inventory
    public bool SetItemWeight(Item item, float weight)
    {
        if (item == null || weight < 0) return false;
        
        InventoryItem inventoryItem = GetInventoryItem(item);
        if (inventoryItem != null)
        {
            inventoryItem.Weight = weight;
            OnInventoryChanged?.Invoke();
            return true;
        }
        
        return false;
    }

    // Reset an item's weight to its default value
    public bool ResetItemWeight(Item item)
    {
        if (item == null) return false;
        
        InventoryItem inventoryItem = GetInventoryItem(item);
        if (inventoryItem != null && inventoryItem.HasCustomWeight)
        {
            inventoryItem.ResetWeight();
            OnInventoryChanged?.Invoke();
            return true;
        }
        
        return false;
    }

    // Get the current weight of an item (either custom or default)
    public float GetItemWeight(Item item)
    {
        if (item == null) return 0f;
        
        InventoryItem inventoryItem = GetInventoryItem(item);
        if (inventoryItem != null)
        {
            return inventoryItem.Weight;
        }
        
        return item.weight; // Return the default weight if item not in inventory
    }

    // Calculate total inventory weight
    public float GetTotalInventoryWeight()
    {
        float totalWeight = 0f;
        
        foreach (var item in inventoryItems)
        {
            totalWeight += item.Weight * item.Quantity;
        }
        
        return totalWeight;
    }

    public InventoryItem GetEquippedItem()
    {
        return equippedItem;
    }

    // Save/Load inventory (basic implementation)
    public void SaveInventory()
    {
        // TODO: Implement saving to PlayerPrefs, JSON, etc.
        Debug.Log("Inventory save not implemented yet");
    }

    public void LoadInventory()
    {
        // TODO: Implement loading from saved data
        Debug.Log("Inventory load not implemented yet");
    }
}