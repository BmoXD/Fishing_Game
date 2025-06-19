using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SellAllButton : InteractiveUIElement
{

    protected override void OnElementClicked()
    {
        base.OnElementClicked();
        SellAllItems();
    }

    private void SellAllItems()
    {
        List<InventoryItem> sellableItems = InventoryManager.Instance.GetAllSellableItems();
        if (sellableItems.Count <= 0)
            return;

        foreach (var item in sellableItems)
            {
                InventoryManager.Instance.SellItem(item);
            }
    }
}