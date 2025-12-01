using UnityEngine;
using UnityEngine.EventSystems;

public class BackgroundClickDetector : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject background;

    private OsuMiniGame gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<OsuMiniGame>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (gameManager != null)
        {
            gameManager.HandleMiss();
        }
    }
}