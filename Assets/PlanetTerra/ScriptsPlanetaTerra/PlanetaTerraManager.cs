using UnityEngine;
using System.Collections;

/// <summary>
/// Gere o Nível 1. Ativa o "Painel" do Minijogo Pac-Man
/// e pausa o jogador 3D.
/// </summary>
public class PlanetaTerraManager : MonoBehaviour
{
    public static bool isMinigameActive = false;

    [Header("Configuração do Minijogo")]
    [Tooltip("O painel de UI (Canvas) que contém o RawImage e a pontuação.")]
    public GameObject pacManMinigamePanel; // O seu 'PacmanMinigamePanel'

    [Tooltip("O contentor 2D (mundo) que tem os Sprites (Pac-Man, etc.).")]
    public GameObject pacmanMinigame; // O seu 'PacmanMinigame' (o contentor)

    [Tooltip("A câmara 2D separada que filma o minijogo.")]
    public Camera minigameCamera; // A 'MinigameCamera'

    [Tooltip("O tempo em segundos até o minijogo começar.")]
    public float timeToStartMinigame = 15f;

    [Header("Configuração do Jogo Principal")]
    [Tooltip("O script de movimento/câmara do seu jogador 3D (para pausar).")]
    public MonoBehaviour mainPlayerController; // O script do seu jogador do Nível 1

    private bool minigameWasStarted = false;

    void Start()
    {
        // Garante que tudo começa desligado
        pacManMinigamePanel.SetActive(false);
        pacmanMinigame.SetActive(false);
        minigameCamera.gameObject.SetActive(false); // Desliga a câmara 2D

        isMinigameActive = false;
        minigameWasStarted = false;

        StartCoroutine(StartMinigameAfterDelay(timeToStartMinigame));
    }

    IEnumerator StartMinigameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!minigameWasStarted)
        {
            OpenPacManMinigame();
        }
    }

    public void OpenPacManMinigame()
    {
        if (pacManMinigamePanel == null || pacmanMinigame == null || minigameCamera == null)
        {
            Debug.LogError("O PlanetaTerraManager não tem o Panel, o World ou a Câmara do Pac-Man configurados!");
            return;
        }

        Debug.Log("A abrir o minijogo Pac-Man!");
        minigameWasStarted = true;
        isMinigameActive = true;

        // 1. Ativa tudo o que é do minijogo
        pacManMinigamePanel.SetActive(true); // Liga o painel de UI
        pacmanMinigame.SetActive(true); // Liga o mundo 2D (física)
        minigameCamera.gameObject.SetActive(true); // Liga a câmara 2D

        // 2. "Pausa" o jogo principal desativando o controlo do jogador
        if (mainPlayerController != null)
        {
            mainPlayerController.enabled = false;
        }

        // Opcional: Mostrar o cursor do rato para jogar na UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ClosePacManMinigame()
    {
        Debug.Log("A fechar o minijogo e a voltar ao Nível 1.");
        isMinigameActive = false;

        // 1. Desativa tudo o que é do minijogo
        pacManMinigamePanel.SetActive(false);
        pacmanMinigame.SetActive(false);
        minigameCamera.gameObject.SetActive(false);

        // 2. "Retoma" o jogo principal
        if (mainPlayerController != null)
        {
            mainPlayerController.enabled = true;
        }

        // Opcional: Esconder o cursor do rato (para jogos 3D FPS)
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }
}