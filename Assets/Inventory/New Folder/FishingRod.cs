using UnityEngine;

public class EatFish : ItemFunctionality
{
    public override void Use()
    {
        Debug.Log("You ate the fish!");
        // Apply healing, buffs, etc.
    }
}
