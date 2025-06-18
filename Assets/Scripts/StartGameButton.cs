using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameButton : InteractiveUIElement
{
    [SerializeField] private string sceneName = "Island";

    protected override void OnElementClicked()
    {
        base.OnElementClicked();
        SceneManager.LoadScene(sceneName);
    }
}
