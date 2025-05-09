using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WaterItemEntry
{
    public Item item;
    [Range(0f, 1f)]
    public float rarity = 0.5f; // 0 = never, 1 = always, in-between = probability
}

public struct WaterCatchResult
{
    public Item item;
    public float weight;
}

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class WaterVolume : MonoBehaviour
{
    [Header("Fishable Items")]
    public List<WaterItemEntry> possibleItems = new List<WaterItemEntry>();

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
        gameObject.tag = "Water";
    }

    /// <summary>
    /// Returns a random item based on rarity weights, or null if none.
    /// </summary>
    public WaterCatchResult GetRandomItem()
    {
        WaterCatchResult result = new WaterCatchResult { item = null, weight = 0f };
        if (possibleItems.Count == 0)
            return result;

        float total = 0f;
        foreach (var entry in possibleItems)
            total += entry.rarity;

        if (total <= 0f)
            return result;

        float rand = Random.value * total;
        float accum = 0f;
        foreach (var entry in possibleItems)
        {
            accum += entry.rarity;
            if (rand <= accum)
            {
                result.item = entry.item;
                if (entry.item != null)
                    result.weight = Random.Range(entry.item.minWeight, entry.item.maxWeight);
                return result;
            }
        }
        // fallback
        var last = possibleItems[possibleItems.Count - 1];
        result.item = last.item;
        if (last.item != null)
            result.weight = Random.Range(last.item.minWeight, last.item.maxWeight);
        return result;
    }
}