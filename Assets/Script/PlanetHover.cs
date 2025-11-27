using UnityEngine;
using TMPro; 

public class PlanetHover : MonoBehaviour
{
    [Header("Settings")]
    public string planetName = "Planet Name"; 
    public float verticalOffset = -1.5f; 

    [Header("References")]
    
    [SerializeField] private GameObject labelObject;
    [SerializeField] private TMP_Text labelText;

    private Camera mainCamera;

    private void Start()
    {
       
        mainCamera = Camera.main;
    }

    private void OnMouseEnter()
    {
        if (labelObject != null)
        {
           
            labelText.text = planetName;

            
            labelObject.SetActive(true);
        }
    }
  
    private void OnMouseOver()
    {
        if (labelObject != null)
        {
            
          
            Vector3 worldPos = transform.position + (Vector3.down * verticalOffset);
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

           
            labelObject.transform.position = screenPos;
        }
    }

    private void OnMouseExit()
    {
        if (labelObject != null)
        {
        
            labelObject.SetActive(false);
        }
    }
}