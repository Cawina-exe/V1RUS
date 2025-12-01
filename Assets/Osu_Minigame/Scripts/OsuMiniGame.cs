using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class OsuMiniGame : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI failText;
    public GameObject winScreen;
    public GameObject lossScreen;
    public GameObject MiniGameOsu;

    [Header("Audio Settings")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioClip clickSound;

    public AudioClip winSound;
    public AudioClip loseSound;

    [Header("Game Setup")]
    public GameObject circlePrefab;
    public float circleRadius = 1.0f;
    public LayerMask circlesLayerMask;

    [Header("Game Rules")]
    public float timeLimit = 30f;
    public int totalCirclesToSpawn = 20;
    public int winScoreRequirement = 17;
    public int maxFails = 3;

    private int currentTargetNumber;
    private int circlesClicked;
    private int currentFails;
    private float currentTime;
    private bool gameIsActive = false;
 
    void Start()
    {
        if (circlesLayerMask == 0)
        {
            circlesLayerMask = LayerMask.GetMask("Circles");
        }
    }

    void Update()
    {
        if (!gameIsActive) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            EndGame(false);
        }

        UpdateUI();
    }

    public void StartGame()
    {
        gameIsActive = true;
        currentTargetNumber = 1;
        circlesClicked = 0;
        currentFails = 0;
        currentTime = timeLimit;

        if (winScreen) winScreen.SetActive(false);
        if (lossScreen) lossScreen.SetActive(false);
        UpdateUI();

        if (musicSource != null)
        {
            musicSource.Play();
        }

        SpawnNextCircle();
    }

    public void PlayClickSound()
    {
        if (sfxSource != null && clickSound != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }

    [SerializeField] private RectTransform localToSpawn;

    void SpawnNextCircle()
    {
        if (!gameIsActive) return;

        int maxSpawnAttempts = 30;
        int attempts = 0;
        Vector3 spawnPos = Vector3.zero;
        bool spotIsClear = false;

        Rect rect = localToSpawn.rect;

        do
        {
            float x = Random.Range(rect.xMin, rect.xMax);
            float y = Random.Range(rect.yMin, rect.yMax);

            Vector2 localPos = new Vector2(x, y);
            spawnPos = localToSpawn.TransformPoint(localPos);

            attempts++;
            if (attempts > maxSpawnAttempts)
            {
                break;
            }

            spotIsClear = CheckIfPositionIsFree(spawnPos);

        } while (!spotIsClear);

        GameObject circleGO = Instantiate(circlePrefab, spawnPos, Quaternion.identity, localToSpawn);
        circleGO.GetComponent<ClickableCircle>().Initialize(this, currentTargetNumber);

        bool CheckIfPositionIsFree(Vector3 worldPos)
        {
            foreach (Transform child in localToSpawn)
            {
                float minDist = circleRadius * 100f;
                float dist = Vector3.Distance(child.position, worldPos);

                if (dist < minDist)
                    return false;
            }

            return true;
        }
    }

    public void CircleClicked(int numberClicked)
    {
        if (!gameIsActive) return;

        if (numberClicked == currentTargetNumber)
        {
            PlayClickSound();

            circlesClicked++;
            currentTargetNumber++;

            UpdateUI();

            if (circlesClicked >= totalCirclesToSpawn)
            {
                EndGame(true);
            }
            else
            {
                SpawnNextCircle();
            }
        }
    }

    public void HandleMiss()
    {
        if (!gameIsActive) return;

        currentFails++;
        UpdateUI();

        if (currentFails >= maxFails)
        {
            EndGame(false);
        }
    }


    void EndGame(bool clickedAllCircles)
    {
        gameIsActive = false;

        if (musicSource != null)
        {
            musicSource.Stop();
        }

      
        ClickableCircle circle = FindObjectOfType<ClickableCircle>();
        if (circle != null)
        {
            Destroy(circle.gameObject);
        }

        bool playerWon = false;

        if (clickedAllCircles)
        {
            playerWon = true;
        }
        else if (circlesClicked >= winScoreRequirement && currentFails < maxFails)
        {
            playerWon = true;
        }


        if (playerWon)
        {
            Virus pontos = GameObject.Find("EventSystem").GetComponent<Virus>();
            pontos.virusPontos += pontos.virusPontos * 0.5f;
            pontos.VirusPontosText.text = pontos.virusPontos.ToString();

            if (winScreen) winScreen.SetActive(true);
            if (sfxSource != null && winSound != null)
            {
                sfxSource.PlayOneShot(winSound);
            }
        }
        else
        {
            if (lossScreen) lossScreen.SetActive(true);
            if (sfxSource != null && loseSound != null)
            {
                sfxSource.PlayOneShot(loseSound);
            }
        }

   
        Invoke("CloseMiniGame", 3f);
    }

  
    public void CloseMiniGame()
    {
      
        MiniGameOsu.SetActive(false);
    }

    void UpdateUI()
    {
        if (timerText) timerText.text = "Time: " + currentTime.ToString("F1");
        if (scoreText) scoreText.text = "Score: " + circlesClicked + " / " + totalCirclesToSpawn;
        if (failText) failText.text = "Fails: " + currentFails + " / " + maxFails;
    }
}