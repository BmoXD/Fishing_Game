using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ItemSlot : InteractiveUIElement
{
    [Header("Item Slot")]
    [SerializeField] protected InventoryItem inventoryItem;
    [SerializeField] protected int slotIndex;
    
    [Header("Bookmark Display")]
    [SerializeField] protected TextMeshProUGUI bookmarkText;
    
    [Header("Quantity Display")]
    [SerializeField] protected TextMeshProUGUI quantityText;
    
    // Properties
    public InventoryItem Item => inventoryItem;
    public int SlotIndex => slotIndex;
    public bool IsEmpty => inventoryItem == null;
    
    // Called to set the slot index
    public virtual void Initialize(int index)
    {
        slotIndex = index;
    }
    
    // Set the inventory item for this slot
    public virtual void SetItem(InventoryItem item)
    {
        inventoryItem = item;
        
        if (item != null)
        {
            // Update icon
            if (iconImage != null && item.ItemData != null)
            {
                iconImage.sprite = item.ItemData.icon;
                iconImage.color = new Color(1, 1, 1, 1); // Make fully visible
            }
            
            // Update quantity
            UpdateQuantityText();
            
            // Update bookmark
            UpdateBookmarkDisplay();
        }
        else
        {
            ClearSlot();
        }
    }
    
    // Clear the slot
    public virtual void ClearSlot()
    {
        inventoryItem = null;
        
        // Reset icon
        if (iconImage != null)
        {
            if (emptyIcon != null)
            {
                iconImage.sprite = emptyIcon;
                iconImage.color = new Color(1, 1, 1, 1);
            }
            else
            {
                iconImage.color = new Color(1, 1, 1, 0); // Make transparent
            }
        }
        
        // Clear quantity
        if (quantityText != null)
        {
            quantityText.text = "";
            quantityText.gameObject.SetActive(false);
        }
        
        // Clear bookmark
        if (bookmarkText != null)
        {
            bookmarkText.text = "";
            bookmarkText.gameObject.SetActive(false);
        }
    }
    
    // Update the quantity text
    protected virtual void UpdateQuantityText()
    {
        if (quantityText != null && inventoryItem != null)
        {
            int quantity = inventoryItem.Quantity;
            if (quantity > 1)
            {
                quantityText.text = quantity.ToString();
                quantityText.gameObject.SetActive(true);
            }
            else
            {
                quantityText.text = "";
                quantityText.gameObject.SetActive(false);
            }
        }
    }
    
    // Update the bookmark display
    protected virtual void UpdateBookmarkDisplay()
    {
        if (bookmarkText != null && inventoryItem != null)
        {
            int bookmark = inventoryItem.BookmarkSlot;
            if (bookmark > 0)
            {
                bookmarkText.text = bookmark.ToString();
                bookmarkText.gameObject.SetActive(true);
            }
            else
            {
                bookmarkText.text = "";
                bookmarkText.gameObject.SetActive(false);
            }
        }
    }
}