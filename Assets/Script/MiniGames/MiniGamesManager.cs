using UnityEngine;

public class MiniGamesManager : MonoBehaviour
{
    private MiniGamePopUp miniGamePopUp;
    private MiniGamePopUp miniGamePopUp1;
    private MiniGamePopUp miniGamePopUp2;
    private float timeRemaining = 300f;
    private bool firstMiniGame = false;
    private bool secondMiniGame = false;

    private int currentMiniGameIndex;

    void Start()
    {
        miniGamePopUp = GameObject.Find("EventSystem").GetComponent<MiniGamePopUp>();
        miniGamePopUp1 = GameObject.Find("EventSystem").GetComponent<MiniGamePopUp>();
        miniGamePopUp2 = GameObject.Find("EventSystem").GetComponent<MiniGamePopUp>();

        SaveData data = SaveSystem.Load();

        currentMiniGameIndex = -1;

        for (int i = 0; i < data.PlanetaAtivado.Count; i++)
        {
            if (data.PlanetaAtivado[i])
            {
                currentMiniGameIndex = i;
                break;
            }
        }

        if (currentMiniGameIndex == -1)
            currentMiniGameIndex = 0;

    }

    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            if (timeRemaining <= 210f && firstMiniGame == false)
            {
                AtivarMiniGame();
                firstMiniGame = true;
            }

            if (timeRemaining <= 90f && secondMiniGame == false)
            {
                AtivarMiniGame();
                secondMiniGame = true;
            }
        }
    }

    private void AtivarMiniGame()
    {
        if (currentMiniGameIndex == 0)
        {
            miniGamePopUp.StartGame();
        }
        else if (currentMiniGameIndex == 1)
        {
            miniGamePopUp1.StartGame();
        }
        else if (currentMiniGameIndex == 2)
        {
            miniGamePopUp2.StartGame();
        }
    }
}
