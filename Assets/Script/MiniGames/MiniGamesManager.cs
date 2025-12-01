using UnityEngine;
using UnityEngine.SceneManagement; 

public class MiniGamesManager : MonoBehaviour
{
    private MiniGamePopUp miniGamePopUp;
    [SerializeField] private GameObject miniGameOsu;
    [SerializeField] private OsuMiniGame osuLogic;

    [Header("Scene Names")]

    public string pacmanSceneName = "Pacman_Minigame";

    private float timeRemaining = 300f;
    private bool firstMiniGame = false;
    private bool secondMiniGame = false;

    private int currentMiniGameIndex;

    public GameObject MiniGameOsu { get => miniGameOsu; set => miniGameOsu = value; }

    void Start()
    {
        MiniGameOsu.SetActive(false);
        miniGamePopUp = GameObject.Find("EventSystem").GetComponent<MiniGamePopUp>();

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

        if (currentMiniGameIndex == -1) currentMiniGameIndex = 0;
    }

    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            if (timeRemaining <= 300f && !firstMiniGame)
            {
                AtivarMiniGame();
                firstMiniGame = true;
            }

            if (timeRemaining <= 250f && !secondMiniGame)
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
            SceneManager.LoadScene(pacmanSceneName, LoadSceneMode.Additive);
        }
        else if (currentMiniGameIndex == 1)
        {
            MiniGameOsu.SetActive(true);
            if(MiniGameOsu != null) osuLogic.StartGame();
        }
        else if (currentMiniGameIndex == 2)
        {
            if (miniGamePopUp != null) miniGamePopUp.StartGame();   
        }
    }
}