using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

public class ShopSellPanel : MonoBehaviour
{
    [Header("Shop UI Elements")]
    [SerializeField] private Transform shopContent; // Parent object for shop item slots
    [SerializeField] private GameObject shopItemSlotPrefab; // Prefab for ShopItemSlot

    private List<InventoryItem> playerItems; // List of items owned by the player

    private void Start()
    {
        PopulateShop();
    }

    private void OnEnable()
    {
        InventoryManager.Instance.OnInventoryChanged += PopulateShop;
    }

    private void OnDisable()
    {
        InventoryManager.Instance.OnInventoryChanged -= PopulateShop;
    }

    // Method to retrieve player's items (this could be replaced with actual inventory retrieval logic)
    private void RetrieveSellablePlayerItems()
    {
        playerItems = InventoryManager.Instance.GetAllSellableItems();
    }

    // Method to populate the shop with ShopItemSlots
    private void PopulateShop()
    {
        RetrieveSellablePlayerItems();

        foreach (Transform child in shopContent)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in playerItems)
        {
            CreateShopItemSlot(item);
        }
    }

    // Method to create a ShopItemSlot for a given InventoryItem
    private void CreateShopItemSlot(InventoryItem item)
    {
        GameObject slotObject = Instantiate(shopItemSlotPrefab, shopContent);
        ShopItemSlot shopItemSlot = slotObject.GetComponent<ShopItemSlot>();
        shopItemSlot.SetItem(item);
    }
}