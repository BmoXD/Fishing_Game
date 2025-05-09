using UnityEngine;
using TMPro;

public class HotbarSlot : ItemSlot
{
    [Header("Hotbar Display")]
    //[SerializeField] private TextMeshProUGUI hotkeyText;
    [SerializeField] private int hotbarIndex; // 1-5 (matching bookmark indexes)

    private HotbarManager hotbarManager;
    
    public int HotbarIndex => hotbarIndex;
    
    public override void Initialize(int index)
    {
        base.Initialize(index);
        hotbarIndex = index;
        // Find HotbarManager and notify it
        hotbarManager = GetComponentInParent<HotbarManager>();
        
        // Set hotkey text if available
        if (bookmarkText != null)
        {
            bookmarkText.text = index.ToString();
        }
    }
    
    // Override selection to inform HotbarManager
    protected override void OnElementClicked()
    {
        if (hotbarManager != null)
        {
            hotbarManager.SelectSlot(slotIndex - 1); // Convert to 0-based index
        }
    }
}