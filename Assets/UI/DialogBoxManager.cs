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
    private Coroutine fadeCoroutine;
    private CanvasGroup canvasGroup;
    private PlayerControls controls;
    private bool isTypewriterActive = false;
    private bool isFadeActive = false;
    private string fullMessage = "";

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

        if (titleText != null)
            titleText.text = title;

        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fullMessage = message;
        isTypewriterActive = false;
        isFadeActive = false;

        if (messageText != null)
            typewriterCoroutine = StartCoroutine(TypewriterEffect(message));
        if (canvasGroup != null)
            fadeCoroutine = StartCoroutine(FadeCanvasGroup(0f, 1f, fadeDuration));
    }

    public void Close()
    {
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
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

    private IEnumerator FadeCanvasGroup(float from, float to, float duration)
    {
        isFadeActive = true;
        float elapsed = 0f;
        canvasGroup.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            if (!isFadeActive) yield break;
            yield return null;
        }
        canvasGroup.alpha = to;
        isFadeActive = false;
    }

    private void OnClickPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        // If fade or typewriter is active, skip both
        if (isFadeActive || isTypewriterActive)
        {
            isFadeActive = false;
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            if (canvasGroup != null)
                canvasGroup.alpha = 1f;

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