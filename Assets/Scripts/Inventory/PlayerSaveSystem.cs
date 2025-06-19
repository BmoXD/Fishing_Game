using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    public int playerMoney;
    public List<InventoryItemData> inventoryItems;
}

[Serializable]
public class InventoryItemData
{
    public int itemId;
    public int quantity;
    public float weight;
    public int bookmarkSlot;
}

public static class PlayerSaveSystem
{
    private static string saveFileName = "FishingGameSave.json";
    public static string SaveFilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), saveFileName);

    public static void SaveGame(int playerMoney, List<InventoryItem> inventoryItems)
    {
        PlayerSaveData data = new PlayerSaveData
        {
            playerMoney = playerMoney,
            inventoryItems = new List<InventoryItemData>()
        };
        foreach (var item in inventoryItems)
        {
            data.inventoryItems.Add(new InventoryItemData
            {
                itemId = item.ItemData.ItemID,
                quantity = item.Quantity,
                weight = item.Weight,
                bookmarkSlot = item.BookmarkSlot
            });
        }
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SaveFilePath, json);
        Debug.Log($"Game saved to {SaveFilePath}");
    }

    public static PlayerSaveData LoadGame()
    {
        if (!File.Exists(SaveFilePath))
        {
            Debug.LogWarning("Save file not found.");
            return null;
        }
        string json = File.ReadAllText(SaveFilePath);
        PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);
        Debug.Log("Game loaded.");
        return data;
    }
}
