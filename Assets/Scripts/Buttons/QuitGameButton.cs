using UnityEngine;

public class QuitGameButton : InteractiveUIElement
{
    protected override void OnElementClicked()
    {
        base.OnElementClicked();
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.SaveInventory();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
