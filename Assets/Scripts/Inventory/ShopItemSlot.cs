using UnityEngine;
using TMPro;

public class ShopItemSlot : ItemSlot
{
    [Header("Price Display")]
    [SerializeField] private TextMeshProUGUI priceText;

    public override void SetItem(InventoryItem item)
    {
        base.SetItem(item);
        UpdatePriceText();
    }

    protected override void OnElementClicked()
    {
        base.OnElementClicked();

        InventoryManager.Instance.SellItem(inventoryItem);
    }

    // Update the price display based on the item's price
    private void UpdatePriceText()
    {
        if (priceText != null && inventoryItem != null)
        {
            float price = inventoryItem.getPrice();
            priceText.text = $"${price:0.##}";
            priceText.gameObject.SetActive(true);
        }
        else
        {
            priceText.text = "";
            priceText.gameObject.SetActive(false);
        }
    }
}