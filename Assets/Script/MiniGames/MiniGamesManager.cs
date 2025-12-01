using UnityEngine;


public class MiniGamesManager : MonoBehaviour
{
    private MiniGamePopUp miniGamePopUp;
    [SerializeField] private GameObject miniGameOsu;
    [SerializeField] private OsuMiniGame osuLogic;

    [Header("Minigame Prefabs")]
    public GameObject pacmanPrefab; 

    private float timeRemaining = 300f;
    private bool firstMiniGame = false;
    private bool secondMiniGame = false;

    private int currentMiniGameIndex;

    public GameObject MiniGameOsu { get => miniGameOsu; set => miniGameOsu = value; }

    void Start()
    {
        if (MiniGameOsu != null) MiniGameOsu.SetActive(false);

        
        GameObject eventSys = GameObject.Find("EventSystem");
        if (eventSys != null) miniGamePopUp = eventSys.GetComponent<MiniGamePopUp>();

        SaveData data = SaveSystem.Load();
        currentMiniGameIndex = -1;

        if (data != null && data.PlanetaAtivado != null)
        {
            for (int i = 0; i < data.PlanetaAtivado.Count; i++)
            {
                if (data.PlanetaAtivado[i])
                {
                    currentMiniGameIndex = i;
                    break;
                }
            }
        }

        if (currentMiniGameIndex == -1) currentMiniGameIndex = 0;
    }

    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            if (timeRemaining <= 210f && !firstMiniGame)
            {
                AtivarMiniGame();
                firstMiniGame = true;
            }

            if (timeRemaining <= 90f && !secondMiniGame)
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
           
            WorldsFunction worldFunc = FindFirstObjectByType<WorldsFunction>();
            if (worldFunc != null) worldFunc.ToggleMainScene(false);

            
            if (pacmanPrefab != null)
            {
                
                Vector3 safeSpawnPos = new Vector3(0, -1000, 0);
                Instantiate(pacmanPrefab, safeSpawnPos, Quaternion.identity);
            }
        }

        else if (currentMiniGameIndex == 1)
        {
            if (MiniGameOsu != null)
            {
                MiniGameOsu.SetActive(true);
                if (osuLogic != null) osuLogic.StartGame();
            }
        }
        else if (currentMiniGameIndex == 2)
        {
            if (miniGamePopUp != null) miniGamePopUp.StartGame();
        }
    }
}