using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// A button element that opens a specific UI panel through the UIManager.
/// Extends InteractiveUIElement to inherit hover, click, and visual effects.
/// </summary>
public class MenuButtonElement : InteractiveUIElement
{
    [Header("Panel Settings")]
    [SerializeField] private GameObject targetPanel;

    protected override void Awake()
    {
        base.Awake();
        
        // Validate required references
        if (targetPanel == null)
        {
            Debug.LogWarning($"MenuButtonElement on {gameObject.name} has no target panel assigned!");
        }
    }

    protected override void OnElementClicked()
    {
        if (targetPanel == null || UIManager.Instance == null)
            return;
            
        // Try opening with action name first
        string actionName = GetPanelActionName();
        if (!string.IsNullOrEmpty(actionName))
        {
            UIManager.Instance.TogglePanel(actionName);
        }
        else
        {
            // Direct open if we can't find the action name
            UIManager.Instance.OpenPanel(targetPanel);
        }
    }

    // Find the input action name for this panel
    private string GetPanelActionName()
    {
        foreach (var panelInfo in UIManager.Instance.GetPanels())
        {
            if (panelInfo.panel == targetPanel)
            {
                return panelInfo.inputActionName;
            }
        }
        return string.Empty;
    }

    // Update the button's target panel
    public void SetTargetPanel(GameObject panel)
    {
        targetPanel = panel;
    }

    // Manually trigger the button's panel action
    public void TriggerPanel()
    {
        OnElementClicked();
    }
}