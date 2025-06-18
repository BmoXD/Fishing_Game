using UnityEngine;
using TMPro;

public class PlayerMoneyCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;

    private void OnEnable()
    {
        UpdateMoneyText();
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += UpdateMoneyText;
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= UpdateMoneyText;
    }

    private void UpdateMoneyText()
    {
        if (moneyText != null && InventoryManager.Instance != null)
        {
            moneyText.text = $"${InventoryManager.Instance.GetMoney()}";
        }
    }
}