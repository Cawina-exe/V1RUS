using UnityEngine;

public class CreditsScroller : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float scrollSpeed = 100f;
    [SerializeField] private float stopOffset = 0f;

    private RectTransform rectTransform;
    private Vector2 startPosition;
    private float stopYPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        startPosition = rectTransform.anchoredPosition;

        stopYPosition = (rectTransform.rect.height / 2) + stopOffset;
    }

    private void OnEnable()
    {

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = startPosition;
        }
    }

    private void Update()
    {

        if (rectTransform.anchoredPosition.y < stopYPosition)
        {

            rectTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
        }
    }
}