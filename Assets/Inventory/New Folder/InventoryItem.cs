using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    [SerializeField] private Item itemData;
    [SerializeField] private int quantity;
    [SerializeField] private int bookmarkSlot; // 0 = not bookmarked, 1-5 = bookmark slots
    [SerializeField] private float customWeight = -1f; // -1 means use default weight

    public Item ItemData => itemData;
    public int Quantity => quantity;
    public int BookmarkSlot 
    { 
        get => bookmarkSlot;
        set => bookmarkSlot = Mathf.Clamp(value, 0, 5);
    }

    // Weight property that returns custom weight if set, otherwise item's default weight
    public float Weight
    {
        get => customWeight >= 0 ? customWeight : itemData.weight;
        set => customWeight = value;
    }

    // Check if using custom weight
    public bool HasCustomWeight => customWeight >= 0;

    public InventoryItem(Item item, int amount = 1, int bookmark = 0)
    {
        itemData = item;
        quantity = amount;
        bookmarkSlot = Mathf.Clamp(bookmark, 0, 5);
        customWeight = -1f; // Default to using item's weight
    }

    public void AddQuantity(int amount)
    {
        quantity += amount;
        if (quantity < 0) quantity = 0;
    }

    public void RemoveQuantity(int amount)
    {
        quantity -= amount;
        if (quantity < 0) quantity = 0;
    }

    // Reset to using default weight
    public void ResetWeight()
    {
        customWeight = -1f;
    }
}