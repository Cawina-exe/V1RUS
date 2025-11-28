using UnityEngine;
using UnityEngine.SceneManagement; 

public class GameOverScreen : MonoBehaviour
{
    
    public void RetryLevel()
    {
        Time.timeScale = 1f; 

      
        SceneManager.LoadScene("Fase");
    }

   
    public void LoadMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("MainMenu"); 
    }

   
    public void QuitGame()
    {
        Debug.Log("Quitting...");
        Application.Quit();
    }
}