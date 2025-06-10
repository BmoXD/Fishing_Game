using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class FishItem : ItemFunctionality
{
    private Transform itemSocket;
    private ParentConstraint parentConstraint;

    [SerializeField] private Renderer fishMeshRenderer;

    public override void Use()
    {

    }

    protected override void OnEnable()
    {
        GameObject[] sockets = GameObject.FindGameObjectsWithTag("Socket");
        GameObject socketObject = null;
        foreach (var obj in sockets)
        {
            if (obj.name == "ItemFishSocket")
            {
                socketObject = obj;
                break;
            }
        }

        if (socketObject == null)
        {
            Debug.LogError("ItemFishSocket object not found!");
            return;
        }

        //TODO: Create new socket that is meant for fish items
        itemSocket = socketObject.transform;

        parentConstraint = gameObject.AddComponent<ParentConstraint>();

        ConstraintSource source = new ConstraintSource
        {
            sourceTransform = itemSocket,
            weight = 1.0f
        };
        parentConstraint.AddSource(source);

        // Only constrain position, not rotation
        parentConstraint.translationAxis = Axis.X | Axis.Y | Axis.Z;
        //parentConstraint.rotationAxis = Axis.None;

        parentConstraint.constraintActive = true;
        parentConstraint.locked = true;

        SetFishTextureAndSize();
    }

    private void SetFishTextureAndSize()
    {
        InventoryItem invItem = null;

        // Try to get from InventoryManager's equipped item (if this is the equipped prefab)
        if (InventoryManager.Instance != null)
        {
            var equipped = InventoryManager.Instance.GetEquippedItem();
            if (equipped != null && equipped.ItemData != null)
            {
                invItem = equipped;
            }
        }
        Debug.Log(invItem);

        if (fishMeshRenderer != null && fishMeshRenderer.material != null)
        {
            fishMeshRenderer.material.SetTexture("_FishItemTexture", invItem.ItemData.icon.texture);
        }

        transform.localScale = new Vector3(0.001f, 0.001f, 0.001f) * invItem.Weight;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
