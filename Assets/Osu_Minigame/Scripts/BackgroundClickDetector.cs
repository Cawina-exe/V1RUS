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
        Debug.Log("Background clicked");
        if (gameManager != null)
        {
            Debug.Log("Notifying game manager of miss");
            gameManager.HandleMiss();
        }
    }
}