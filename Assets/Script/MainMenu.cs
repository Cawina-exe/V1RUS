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
    [SerializeField] private GameObject solarSystem;

    [Header("UI Elements")]
    [SerializeField] private Slider volumeSliderMusic;
    [SerializeField] private Slider volumeSliderSFX;
    [SerializeField] private AudioSource audioSourceMusic;
    [SerializeField] private AudioSource audioSourceSFX;

    private SaveData currentData;

    private void Start()
    {
        currentData = SaveSystem.Load();

        if (volumeSliderMusic != null)
        {
            volumeSliderMusic.value = currentData.som;
        }

        if (volumeSliderSFX != null)
        {
            volumeSliderSFX.value = currentData.sfx;
        }

        if (mainMenuUI != null) mainMenuUI.SetActive(true);
        if (optionsUI != null) optionsUI.SetActive(false);
        if (faseUI != null) faseUI.SetActive(false);
        if (creditsUI != null) creditsUI.SetActive(false);
        if(solarSystem != null) solarSystem.SetActive(false);
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

    public void SetVolumeMusic(float volume)
    {
        audioSourceMusic.volume = volume;

        currentData.som = volume;

        SaveSystem.Save(currentData);
    }

    public void SetVolumeSFX(float volume)
    {
        audioSourceSFX.volume = volume;

        currentData.sfx = volume;

        SaveSystem.Save(currentData);
    }

    public void OpenMainMenu()
    {
        faseUI.SetActive(false);
        optionsUI.SetActive(false);
        creditsUI.SetActive(false);
        mainMenuUI.SetActive(true);
        solarSystem.SetActive(false);
    }

    public void CloseMainMenu()
    {
        mainMenuUI.SetActive(false);
        solarSystem.SetActive(true);
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