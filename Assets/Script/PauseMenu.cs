using UnityEngine;
using UnityEngine.SceneManagement; // Required to change scenes

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI; // Assign your PausePanel here
    public GameObject optionsUI;

    void Update()
    {
        // Toggle pause when pressing Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Resumes time
        GameIsPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Freezes time
        GameIsPaused = true;
    }

     public void Options()
    {
        optionsUI.SetActive(true);
        pauseMenuUI.SetActive(false);
    }
    public void OutOfOptions()
    {
        optionsUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }
    public void LoadMenu()
    {
        Time.timeScale = 1f; // crucial: reset time before leaving scene
        SceneManager.LoadScene("MainMenu"); // Make sure your menu scene is named exactly this
        
    }
}