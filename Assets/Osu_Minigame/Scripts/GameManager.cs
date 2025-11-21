using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI failText;
    public GameObject winScreen;
    public GameObject lossScreen;

    
    [Header("Audio Settings")]
    public AudioSource musicSource; 
    public AudioSource sfxSource;  
    public AudioClip clickSound;   

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
        StartGame();
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

    void StartGame()
    {
        gameIsActive = true;
        currentTargetNumber = 1;
        circlesClicked = 0;
        currentFails = 0;
        currentTime = timeLimit;

        winScreen.SetActive(false);
        lossScreen.SetActive(false);
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

    void SpawnNextCircle()
    {
        if (!gameIsActive) return;

        int maxSpawnAttempts = 20;
        int currentSpawnAttempts = 0;
        Vector2 spawnPos = Vector2.zero;
        bool spotIsClear = false;

        do
        {
            float randomX = Random.Range(-7f, 7f);
            float randomY = Random.Range(-4f, 4f);
            spawnPos = new Vector2(randomX, randomY);

            currentSpawnAttempts++;
            if (currentSpawnAttempts > maxSpawnAttempts)
            {
                Debug.LogError("Could not find a clear spot to spawn circle " + currentTargetNumber);
                break;
            }

            spotIsClear = (Physics2D.OverlapCircle(spawnPos, circleRadius, circlesLayerMask) == null);

        } while (!spotIsClear);

        if (spotIsClear)
        {
            GameObject circleGO = Instantiate(circlePrefab, spawnPos, Quaternion.identity);
            circleGO.GetComponent<ClickableCircle>().Initialize(this, currentTargetNumber);
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

        if (clickedAllCircles)
        {
            winScreen.SetActive(true);
        }
        else if (circlesClicked >= winScoreRequirement && !clickedAllCircles)
        {
            winScreen.SetActive(true);
        }
        else
        {
            lossScreen.SetActive(true);
        }
    }

    void UpdateUI()
    {
        timerText.text = "Time: " + currentTime.ToString("F1");
        scoreText.text = "Score: " + circlesClicked + " / " + totalCirclesToSpawn;
        failText.text = "Fails: " + currentFails + " / " + maxFails;
    }
}
//Lógicas em falta: Butão de voltar para o jogo após perder o minijogo e Butão de receber o buff depois de ganhar o minijogo
//Falta o UI ( Imagens e sprites), a Fonte do texto está implementada