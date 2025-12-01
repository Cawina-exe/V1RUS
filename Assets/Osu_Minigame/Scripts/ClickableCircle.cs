using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ClickableCircle : MonoBehaviour, IPointerClickHandler
{
    public int circleNumber;
    private OsuMiniGame gameManager;
    private bool isClicked = false;

    private Image image; 

    void Awake()
    {
        image = GetComponent<Image>();
        if (image == null)
        {
        }
    }

    public void Initialize(OsuMiniGame manager, int number)
    {
        gameManager = manager;
        circleNumber = number;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isClicked) return;
        isClicked = true;

        gameManager.CircleClicked(circleNumber);
        StartCoroutine(PopAndShrinkAnimation());
    }

    private IEnumerator PopAndShrinkAnimation()
    {
        float popDuration = 0.1f;
        float shrinkDuration = 0.2f;
        float popScaleMultiplier = 1.3f;

        float darkenAmount = 0.7f;

        Vector3 startScale = transform.localScale;
        Vector3 popScale = startScale * popScaleMultiplier;
        Vector3 endScale = Vector3.zero;

        Color startColor = image.color;

        Color popColor = new Color(
            startColor.r * darkenAmount,
            startColor.g * darkenAmount,
            startColor.b * darkenAmount,
            startColor.a
        );

        Color endColor = new Color(popColor.r, popColor.g, popColor.b, 0f);

        float time = 0f;

        while (time < popDuration)
        {
            float t = time / popDuration;
            transform.localScale = Vector3.Lerp(startScale, popScale, t);
            image.color = Color.Lerp(startColor, popColor, t);
            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = popScale;
        image.color = popColor;

        time = 0f;

        while (time < shrinkDuration)
        {
            float t = time / shrinkDuration;
            transform.localScale = Vector3.Lerp(popScale, endScale, t);
            image.color = Color.Lerp(popColor, endColor, t);
            time += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}