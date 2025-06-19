using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButton : InteractiveUIElement
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    protected override void OnElementClicked()
    {
        base.OnElementClicked();
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SaveInventory();
            Destroy(InventoryManager.Instance.gameObject);
        }
        if (UIManager.Instance != null)
            Destroy(UIManager.Instance.gameObject);
            
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
