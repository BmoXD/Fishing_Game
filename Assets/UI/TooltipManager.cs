using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    [Header("Tooltip UI References")]
    [SerializeField] private GameObject tooltipRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private TMP_Text equipText;
    [SerializeField] private TMP_Text weightText;
    [SerializeField] private TMP_Text worthText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        HideTooltip();
    }

    public void ShowTooltip(string title, string desc, bool showEquip, string weight, string worth)
    {
        tooltipRoot.SetActive(true);
        titleText.text = string.IsNullOrEmpty(title) ? "Empty" : title;
        descText.text = string.IsNullOrEmpty(title) ? "All meow no bite" : desc;
        equipText.gameObject.SetActive(showEquip);
        weightText.text = string.IsNullOrEmpty(weight) ? "" : weight + "kg";
        worthText.text = string.IsNullOrEmpty(worth) ? "" : "Worth $"+worth;
    }

    public void HideTooltip()
    {
        tooltipRoot.SetActive(false);
    }
}