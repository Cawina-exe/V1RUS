using UnityEngine;

public class AtivarJogo : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;

    [SerializeField] private bool toggle;

    void OnMouseDown()
    {
        if (targetObject != null)
        {
            if (toggle)
            {
                targetObject.SetActive(!targetObject.activeSelf);
            }
            else
            {
                targetObject.SetActive(true);
            }
        }
    }
}