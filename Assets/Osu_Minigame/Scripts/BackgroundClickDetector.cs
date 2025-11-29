using UnityEngine;

public class BackgroundClickDetector : MonoBehaviour
{
    private OsuMiniGame gameManager;

    void Start()
    {
       
        gameManager = FindObjectOfType<OsuMiniGame>();
    }

 
    private void OnMouseDown()
    {
        
        gameManager.HandleMiss();
    }
}