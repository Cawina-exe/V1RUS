using UnityEngine;
using TMPro; // IMPORTANTE: Mude para 'using UnityEngine.UI;' se estiver a usar Text normal

/// <summary>
/// Gere o estado INTERNO do minijogo Pac-Man.
/// O NOME DA CLASSE DEVE SER IGUAL AO NOME DO FICHEIRO: _PacmanManager
/// </summary>
public class _PacmanManager : MonoBehaviour
{
    // --- Referências do Minijogo (Arrastar no Inspector) ---
    [Tooltip("O texto da pontuação (TMP ou UI Text).")]
    public TextMeshProUGUI scoreText; // IMPORTANTE: Mude para 'public Text scoreText;' se usar UI Text

    [Tooltip("O painel que aparece quando se ganha.")]
    public GameObject winPanel;

    [Tooltip("O painel que aparece quando se perde.")]
    public GameObject losePanel;

    [Tooltip("O script do jogador Pac-Man (o GameObject Pacman_Player).")]
    public Pacman_Player playerController; // <- MUDADO para Pacman_Player

    // --- Referência ao Jogo Principal (Arrastar no Inspector) ---
    [Tooltip("Arraste o _PlanetaTerraManager para aqui.")]
    public PlanetaTerraManager levelOneManager;

    private int score = 0;
    private int totalPellets;

    /// <summary>
    /// Chamado sempre que o GameObject (PacmanMinigame) é ativado.
    /// Perfeito para reiniciar o jogo.
    /// </summary>
    void OnEnable()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        // Reseta a UI
        winPanel.SetActive(false);
        losePanel.SetActive(false);

        // Reseta o jogador
        if (playerController != null)
        {
            playerController.gameObject.SetActive(true); // Garante que está visível
            playerController.enabled = true; // Garante que se pode mover
            // TODO: Mover o Pac-Man para a posição inicial
            // Ex: playerController.transform.position = new Vector3(0, 0, 0);
        }

        // Reativa todos os pellets
        // Usamos FindObjectsOfType<Pellet>(true) para incluir os inativos
        var pellets = FindObjectsOfType<Pellet>(true);
        foreach (var pellet in pellets)
        {
            pellet.gameObject.SetActive(true);
        }
        totalPellets = pellets.Length; // Define a contagem total

        // Reseta a pontuação
        score = 0;
        UpdateScoreUI();

        if (totalPellets == 0)
        {
            Debug.LogWarning("Não foram encontrados 'Pellets' (com a Tag 'Pellet') no minijogo.");
        }
    }

    /// <summary>
    /// Chamado pelo Pellet quando é comido.
    /// </summary>
    public void PelletEaten(int points)
    {
        score += points;
        totalPellets--;
        UpdateScoreUI();

        // Verifica se o jogador ganhou
        if (totalPellets <= 0)
        {
            WinGame();
        }
    }

    /// <summary>
    /// Chamado pelo Fantasma quando apanha o jogador.
    /// </summary>
    public void PlayerCaught()
    {
        losePanel.SetActive(true);
        if (playerController != null)
        {
            playerController.gameObject.SetActive(false); // Esconde o Pac-Man
        }

        // Invoca o fecho da janela após 2 segundos
        Invoke("CloseMinigame", 2f);
    }

    private void WinGame()
    {
        winPanel.SetActive(true);
        if (playerController != null)
        {
            playerController.enabled = false; // Para o Pac-Man de se mover
        }

        // Invoca o fecho da janela após 2 segundos
        Invoke("CloseMinigame", 2f);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    /// <summary>
    /// Pede ao gestor principal para fechar o minijogo.
    /// </summary>
    private void CloseMinigame()
    {
        // Cancela invokes anteriores (caso ganhe e perca ao mesmo tempo)
        CancelInvoke("CloseMinigame");

        if (levelOneManager != null)
        {
            levelOneManager.ClosePacManMinigame();
        }
        else
        {
            Debug.LogError("O PacManGameManager não tem referência ao levelOneManager!");
        }
    }
}