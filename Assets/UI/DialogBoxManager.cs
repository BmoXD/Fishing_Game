using UnityEngine;
using TMPro;
using System.Collections;

public class DialogBox : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float typewriterSpeed = 0.03f;
    [SerializeField] private float fadeDuration = 0.3f;

    private Coroutine typewriterCoroutine;
    private CanvasGroup canvasGroup;
    private PlayerControls controls;
    private bool isTypewriterActive = false;
    private bool isFadeActive = false;
    private string fullMessage = "";
    private LTDescr fadeTween; // Add this field to track the LeanTween tween
    private RectTransform transform;
    private Vector2 originalPos;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.UI.Click.Enable();
        controls.UI.Click.started += OnClickPerformed;
    }

    private void OnDisable()
    {
        controls.UI.Click.started -= OnClickPerformed;
        controls.UI.Click.Disable();
    }

    public void Show(string title, string message)
    {
        gameObject.SetActive(true);
        PlayerEvents.RaiseDialogBoxStateChanged(true);

        if (titleText != null)
            titleText.text = title;

        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        // Cancel any running LeanTween fade
        if (fadeTween != null)
            LeanTween.cancel(gameObject);

        fullMessage = message;
        isTypewriterActive = false;
        isFadeActive = false;

        if (messageText != null)
            typewriterCoroutine = StartCoroutine(TypewriterEffect(message));
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            fadeTween = LeanTween.alphaCanvas(canvasGroup, 1f, fadeDuration)
                .setIgnoreTimeScale(true)
                .setOnStart(() => isFadeActive = true)
                .setOnComplete(() => isFadeActive = false);
            
            float animateDist = 10;
            transform = gameObject.GetComponent<RectTransform>();
            originalPos = transform.anchoredPosition;
            Vector2 startPos = transform.anchoredPosition;
            startPos.y -= animateDist;
            transform.anchoredPosition = startPos;

            LeanTween.moveY(transform, transform.anchoredPosition.y + animateDist, 0.5f).setIgnoreTimeScale(true);
        }
    }

    public void Close()
    {
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        // Cancel any running LeanTween fade
        if (fadeTween != null)
            LeanTween.cancel(gameObject);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
        PlayerEvents.RaiseDialogBoxStateChanged(false);
    }

    private IEnumerator TypewriterEffect(string message)
    {
        isTypewriterActive = true;
        messageText.text = "";
        foreach (char c in message)
        {
            messageText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
            if (!isTypewriterActive) yield break;
        }
        isTypewriterActive = false;
    }

    private void OnClickPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        // If fade or typewriter is active, skip both
        if (isFadeActive || isTypewriterActive)
        {
            isFadeActive = false;
            if (fadeTween != null)
                LeanTween.cancel(gameObject);
            if (canvasGroup != null)
                canvasGroup.alpha = 1f;
            transform.anchoredPosition = originalPos;

            isTypewriterActive = false;
            if (typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);
            if (messageText != null)
                messageText.text = fullMessage;

            return;
        }

        if (!isFadeActive && !isTypewriterActive)
        {
            UIManager.Instance.CloseDialog();
        }
    }
}