using UnityEngine;
using UnityEngine.SceneManagement; 

public class VictoryScreen : MonoBehaviour
{
 
    public void GoToMainMenu()
    {
      
        Time.timeScale = 1f;

     
        SceneManager.LoadScene("MainMenu");
    }

    
    public void QuitGame()
    {
        Application.Quit();
    }
}