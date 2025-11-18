using UnityEngine;

public class BackgroundClickDetector : MonoBehaviour
{
    private GameManager gameManager;

    void Start()
    {
       
        gameManager = FindObjectOfType<GameManager>();
    }

 
    private void OnMouseDown()
    {
        
        gameManager.HandleMiss();
    }
}