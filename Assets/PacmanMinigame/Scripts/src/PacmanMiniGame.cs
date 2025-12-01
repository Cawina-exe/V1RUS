using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Pacman
{
    public class PacmanMiniGame : MonoBehaviour
    {
        public static PacmanMiniGame Instance { get; private set; }

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
        public AudioSource musicSource;
        public AudioSource sfxSource;

        public AudioClip pelletClip;
        public AudioClip winClip;
        public AudioClip loseClip;

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
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

    
            GameObject[] pellets = GameObject.FindGameObjectsWithTag("Pellet");
            totalPellets = pellets.Length;
            eatenPellets = 0;

            if (winScreen != null) winScreen.SetActive(false);
            if (loseScreen != null) loseScreen.SetActive(false);

            UpdateScoreUI();
            Debug.Log($"Pacman Game Started! Pellets to eat: {totalPellets}");
        }

        public void PelletEaten(int pointValue)
        {
            if (isGameOver || isVictory) return;

            eatenPellets++;
            UpdateScoreUI();

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

            if (musicSource != null) musicSource.Stop();
            if (sfxSource != null && loseClip != null) sfxSource.PlayOneShot(loseClip);

            if (loseScreen != null) loseScreen.SetActive(true);

      
            Invoke("CloseMiniGame", 3f);
        }

        private void WinGame()
        {
            Debug.Log("VICTORY!");
            isVictory = true;

            if (musicSource != null) musicSource.Stop();
            if (sfxSource != null && winClip != null) sfxSource.PlayOneShot(winClip);

            if (winScreen != null) winScreen.SetActive(true);

            Virus pontos = GameObject.Find("EventSystem").GetComponent<Virus>();
            pontos.virusPontos += pontos.virusPontos * 0.5f;
            pontos.VirusPontosText.text = pontos.virusPontos.ToString();

            Invoke("CloseMiniGame", 3f);
        }

       
        private void CloseMiniGame()
        {
           
            SceneManager.UnloadSceneAsync(gameObject.scene);
        }

        private void UpdateScoreUI()
        {
            if (scoreText != null)
            {
                scoreText.text = $"Pellets: {eatenPellets}/{totalPellets}";
            }
        }
    }
}