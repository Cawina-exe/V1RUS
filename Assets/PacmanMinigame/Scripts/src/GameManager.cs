using UnityEngine;
using UnityEngine.SceneManagement; // For restarting/loading scenes

namespace Pacman
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        public int eatedPellets;
        public int totalPellets;
        public bool isGameOver = false;
        public bool isVictory = false;

        [Header("UI (Optional)")]
        // Assign these in Inspector if you have Text meshes
        //public TMP_Text scoreText;
        //public eatedPellets;
        public GameObject winScreen;
        public GameObject loseScreen;

        private void Awake()
        {
            // Singleton pattern: Ensure only one GameManager exists
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            // Count all pellets in the scene automatically
            GameObject[] pellets = GameObject.FindGameObjectsWithTag("Pellet");

            // ============================================
            // Optional: Update UI here if you implement it
            // ============================================
            // if (scoreText != null) scoreText.text = $"Pellets: {eatedPellets}/{totalPellets}";

            // Set totalPellets and eatedPellets
            totalPellets = pellets.Length;
            eatedPellets = 0;

            Debug.Log($"Game Started! Pellets to eat: {eatedPellets}");
        }

        public void PelletEaten(int pointValue)
        {
            if (isGameOver || isVictory) return;

            // 2. Update State
            eatedPellets++;

            Debug.Log($"Pellets (consumed/total): {eatedPellets}/{totalPellets}");

            // 3. Check Win Condition
            if (eatedPellets == totalPellets)
            {
                WinGame();
            }
        }

        public void HandleDeath()
        {
            if (isVictory) return;

            Debug.Log("GAME OVER! The Virus was caught.");
            isGameOver = true;

            //=======================
            // TODO: Game Over Screen
            //=======================

            // Show Lose UI here...
            // Time.timeScale = 0; // Pause game
        }

        private void WinGame()
        {
            Debug.Log("VICTORY! All pellets consumed.");
            isVictory = true;

            //=====================
            // TODO: Victory Screen
            //=====================

            // Show Win UI here...
            // Time.timeScale = 0; // Pause game
        }
    }
}