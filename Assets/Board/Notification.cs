using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
public class Notification : MonoBehaviour
{
    public static Notification Instance;

    public TMP_Text notifText;
    public Image backgroundImage;
    public CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        notifText = GetComponentInChildren<TMP_Text>();
        backgroundImage = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    public void Show(string message, float duration = 2f)
    {
        Show(message, Color.white, duration);
    }

    public void Show(string message, Color textColor, float duration = 2f)
    {
        if (notifText != null)
        {
            notifText.text = message;
            notifText.color = textColor;
        }

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeRoutine(duration));
    }

    private IEnumerator FadeRoutine(float duration)
    {
        float t = 0f;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.15f);
            yield return null;
        }
        if (canvasGroup != null) canvasGroup.alpha = 1f;
        yield return new WaitForSeconds(duration);
        t = 0f;
        float fadeOutTime = 0.5f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeOutTime);
            yield return null;
        }
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }
}