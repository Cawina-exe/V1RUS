using UnityEngine;

public class MiniGamesManager : MonoBehaviour
{
    private MiniGamePopUp miniGamePopUp;
    private float timeRemaining = 300f;
    private bool firstMiniGame = false;
    private bool secondMiniGame = false;

    void Start()
    {
        miniGamePopUp = GameObject.Find("EventSystem").GetComponent<MiniGamePopUp>();
    }

    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            if (timeRemaining <= 300f && firstMiniGame == false)
            {
                miniGamePopUp.StartGame();
                firstMiniGame = true;
            }

            if (timeRemaining <= 90f && secondMiniGame == false)
            {
                miniGamePopUp.StartGame();
                secondMiniGame = true;
            }
        }
    }
}
