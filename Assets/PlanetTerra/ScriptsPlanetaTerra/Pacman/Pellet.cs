using UnityEngine;

// O NOME DA CLASSE DEVE SER IGUAL AO NOME DO FICHEIRO: Pellet
public class Pellet : MonoBehaviour
{
    [Tooltip("Arraste o _PacmanManager para aqui.")]
    public _PacmanManager gameManager; // <- MUDADO para _PacmanManager

    public int points = 10;

    void Start()
    {
        // Tenta encontrar o GameManager automaticamente se não for arrastado
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<_PacmanManager>(); // <- MUDADO para _PacmanManager
        }
    }

    /// <summary>
    /// Chamado quando outro collider entra neste (requer 'Is Trigger' no Collider2D)
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se foi o Pac-Man (com a Tag "Player") que colidiu
        if (other.CompareTag("Player"))
        {
            if (gameManager != null)
            {
                gameManager.PelletEaten(points);
            }

            // Desativa-se em vez de se destruir.
            // Isto permite ao PacManGameManager reativá-lo na próxima vez.
            gameObject.SetActive(false);
        }
    }
}