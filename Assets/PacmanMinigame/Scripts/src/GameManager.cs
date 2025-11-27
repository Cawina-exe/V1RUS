using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Pacman
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        public int eatenPellets = 0;
        public int totalPellets = 0;
        public bool isGameOver = false;
        public bool isVictory = false;

        [Header("UI References")]
        public TMP_Text scoreText;
        public GameObject winScreen;
        public GameObject loseScreen;

        [Header("Audio")]
        public AudioSource musicSource; // Drag "MusicPlayer" here
        public AudioSource sfxSource;   // Drag "GameManager" itself here

        public AudioClip pelletClip;    // Drag "Waka/Coin" sound here
        public AudioClip winClip;       // Drag "Victory" sound here
        public AudioClip loseClip;      // Drag "Death" sound here

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            // 1. Setup Audio
            // Ensure we have an AudioSource for SFX if user forgot
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

            // 2. Count pellets
            GameObject[] pellets = GameObject.FindGameObjectsWithTag("Pellet");
            totalPellets = pellets.Length;
            eatenPellets = 0;

            UpdateScoreUI();
            Debug.Log($"Game Started! Pellets to eat: {totalPellets}");
        }

        public void PelletEaten(int pointValue)
        {
            if (isGameOver || isVictory) return;

            eatenPellets++;
            UpdateScoreUI();

            // PLAY SOUND: Use PlayOneShot so sounds can overlap (waka-waka style)
            if (sfxSource != null && pelletClip != null)
            {
                sfxSource.PlayOneShot(pelletClip);
            }

            if (eatenPellets >= totalPellets)
            {
                WinGame();
            }
        }

        public void HandleDeath()
        {
            if (isVictory) return;

            Debug.Log("GAME OVER!");
            isGameOver = true;

            // Stop Music, Play Death Sound
            if (musicSource != null) musicSource.Stop();
            if (sfxSource != null && loseClip != null) sfxSource.PlayOneShot(loseClip);

            if (loseScreen != null) loseScreen.SetActive(true);
        }

        private void WinGame()
        {
            Debug.Log("VICTORY!");
            isVictory = true;

            // Stop Music, Play Win Sound
            if (musicSource != null) musicSource.Stop();
            if (sfxSource != null && winClip != null) sfxSource.PlayOneShot(winClip);

            if (winScreen != null) winScreen.SetActive(true);
        }

        private void UpdateScoreUI()
        {
            if (scoreText != null)
            {
                scoreText.text = $"Pellets: {eatenPellets}/{totalPellets}";
            }
        }

        public void RestartGame()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}