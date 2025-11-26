using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject faseUI;

    public void OpenMainMenu()
    {
        faseUI.SetActive(false);
        mainMenuUI.SetActive(true);
    }
    public void CloseMainMenu()
    {
        mainMenuUI.SetActive(false);
    }
    public void closeFase()
    {
        faseUI.SetActive(false);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

}
