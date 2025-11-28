using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject faseUI;
    [SerializeField] private GameObject optionsUI;
    [SerializeField] private GameObject creditsUI;

    [Header("UI Elements")]
    [SerializeField] private Slider volumeSlider;

    private SaveData currentData;

    private void Start()
    {
        currentData = SaveSystem.Load();

  
        AudioListener.volume = currentData.som;

      
        if (volumeSlider != null)
        {
            volumeSlider.value = currentData.som;
        }

       
        if (mainMenuUI != null) mainMenuUI.SetActive(true);
        if (optionsUI != null) optionsUI.SetActive(false);
        if (faseUI != null) faseUI.SetActive(false);
        if (creditsUI != null) creditsUI.SetActive(false);
    }

    public void OpenCredits()
    {
        mainMenuUI.SetActive(false);
        creditsUI.SetActive(true);
    }

    public void CloseCredits()
    {
        creditsUI.SetActive(false);
        mainMenuUI.SetActive(true);
    }

    public void OpenOptions()
    {
        mainMenuUI.SetActive(false);
        optionsUI.SetActive(true);
    }

    public void CloseOptions()
    {
       
        SaveSystem.Save(currentData);

        optionsUI.SetActive(false);
        mainMenuUI.SetActive(true);
    }



    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;

        currentData.som = volume;

        SaveSystem.Save(currentData);
    }



    public void OpenMainMenu()
    {
        faseUI.SetActive(false);
        optionsUI.SetActive(false);
        creditsUI.SetActive(false);
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

    public void GoToMainMenuScene()
    {
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();

    }
}