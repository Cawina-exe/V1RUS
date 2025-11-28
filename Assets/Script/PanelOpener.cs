using UnityEngine;

public class PanelOpener : MonoBehaviour
{
    public GameObject panel; 

    public void OpenPanel()
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    public void ClosePanel()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
}