using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoadingIndicator : MonoBehaviour
{
    private RectTransform _canvas;

    public RectTransform AnimatedElement;
    public RectTransform Background;
    public Text TitleText;
    public Text SubtitleText;
    public AnimationCurve FullscreenAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public string Title
    {
        get { return TitleText.text; }
        set { TitleText.text = value; }
    }

    public string Subtitle
    {
        get { return SubtitleText.text; }
        set { SubtitleText.text = value; }
    }

    private void Start()
    {
        _canvas = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        StartCoroutine(ChangeText());
        StartCoroutine(RotateElement());
        StartCoroutine(MakeFullscreen());
    }

    private IEnumerator ChangeText()
    {
        Subtitle = "downloading...";
        yield return new WaitForSeconds(2.8f);

        Subtitle = "creating savegame...";
        yield return new WaitForSeconds(2);

        Subtitle = "almost ready...";
        yield return new WaitForSeconds(3.2f);

        Subtitle = "let's go";
        yield return Application.LoadLevelAdditiveAsync(1);

        _canvas.gameObject.SetActive(false);
    }

    private IEnumerator RotateElement()
    {
        while (true)
        {
            AnimatedElement.eulerAngles = new Vector3(0, 0, Mathf.Cos(2 * Time.time % Mathf.PI) * 180 - 90);
            yield return null;
        }
    }

    private IEnumerator MakeFullscreen()
    {
        yield return new WaitForSeconds(3);

        var startSize = Background.sizeDelta;
        var targetSize = new Vector2(_canvas.rect.width + 100, _canvas.rect.height + 100);

        var startTime = Time.time;
        var duration = 1.5f;

        while (Time.time - startTime < duration)
        {
            var t = (Time.time - startTime) / duration;
            Background.sizeDelta = Vector2.Lerp(startSize, targetSize, FullscreenAnimationCurve.Evaluate(t));
            yield return null;
        }

        Background.sizeDelta = targetSize;
    }
}
