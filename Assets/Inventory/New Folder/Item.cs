using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Item")]
public class Item : ScriptableObject
{
    private static int lastAssignedID = -1;

    [SerializeField, HideInInspector] private int itemID = -1;
    public int ItemID => itemID;

    public string itemName;
    public Sprite icon;
    public GameObject prefab;
    [TextArea] public string description;
    public float weight;
    public ItemType type;

    public float basePricePerGram = 1f; // Price for 1 gram
    public float minWeight = 1f;        // Minimum possible weight
    public float maxWeight = 1000f;     // Maximum possible weight

    [SerializeField] private bool deleteOnConsume;

    public bool DeleteOnConsume => type == ItemType.Consumable && deleteOnConsume;

    // Attach a script as MonoBehaviour (e.g. FishingRodFunctionality.cs)
    public MonoScript functionalityScript;

    public List<string> onScreenHints;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Auto-assign unique ID only on creation
        if (itemID == -1)
        {
            itemID = GenerateNextID();
            EditorUtility.SetDirty(this);
        }
    }

    private static int GenerateNextID()
    {
        string[] guids = AssetDatabase.FindAssets("t:Item");
        int maxID = -1;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Item existingItem = AssetDatabase.LoadAssetAtPath<Item>(path);
            if (existingItem != null && existingItem.itemID > maxID)
            {
                maxID = existingItem.itemID;
            }
        }

        return maxID + 1;
    }
#endif
}
