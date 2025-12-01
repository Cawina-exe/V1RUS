using UnityEngine;
using TMPro;
using System.Collections;

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

        [Header("UI References (Optional)")]
        [Tooltip("Leave empty if you want the game to close immediately on finish.")]
        public TMP_Text scoreText;
        public GameObject winScreen;
        public GameObject loseScreen;

        [Header("Audio (Optional)")]
        public AudioSource musicSource;
        public AudioSource sfxSource;
        public AudioClip pelletClip;
        public AudioClip winClip;
        public AudioClip loseClip;

      
        private WorldsFunction _mainGame;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
          
            _mainGame = FindFirstObjectByType<WorldsFunction>();
            if (_mainGame == null) Debug.LogError("Pacman: Could not find 'WorldsFunction' in the scene!");

          
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

           
            GameObject[] pellets = GameObject.FindGameObjectsWithTag("Pellet");
            totalPellets = pellets.Length;
            eatenPellets = 0;

            if (winScreen != null) winScreen.SetActive(false);
            if (loseScreen != null) loseScreen.SetActive(false);
            UpdateScoreUI();
        }

        public void PelletEaten(int pointValue)
        {
            if (isGameOver || isVictory) return;

            eatenPellets++;
            UpdateScoreUI();

            if (sfxSource && pelletClip) sfxSource.PlayOneShot(pelletClip);

            if (eatenPellets >= totalPellets)
            {
                WinGame();
            }
        }

        public void HandleDeath()
        {
            if (isVictory) return;
            isGameOver = true;

          
            if (musicSource) musicSource.Stop();
            if (sfxSource && loseClip) sfxSource.PlayOneShot(loseClip);
            if (loseScreen) loseScreen.SetActive(true);

         
            Invoke("FinalizeLoss", 3f);
        }

        private void WinGame()
        {
            isVictory = true;

         
            if (musicSource) musicSource.Stop();
            if (sfxSource && winClip) sfxSource.PlayOneShot(winClip);
            if (winScreen) winScreen.SetActive(true);

          
            Invoke("FinalizeWin", 3f);
        }

        private void FinalizeWin()
        {
            
            if (_mainGame != null) _mainGame.ApplyPacmanWin();

          
            Destroy(gameObject);
            Destroy(transform.root.gameObject);
        }

        private void FinalizeLoss()
        {
            if (_mainGame != null) _mainGame.ApplyPacmanLoss();

           
            Debug.Log("Destroying Pacman Minigame..."); 
            Destroy(gameObject);
            Destroy(transform.root.gameObject);
        }

       

        private IEnumerator CloseRoutine(float delay)
        {
           
            yield return new WaitForSecondsRealtime(delay);

          
            if (_mainGame != null)
            {
                if (isVictory) _mainGame.ApplyPacmanWin();
                else _mainGame.ApplyPacmanLoss();
            }

            Debug.Log("Pacman: Closing minigame.");
            Destroy(gameObject);
        }

        private void UpdateScoreUI()
        {
            if (scoreText != null) scoreText.text = $"Pellets: {eatenPellets}/{totalPellets}";
        }
    }
}