using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
// O NOME DA CLASSE DEVE SER IGUAL AO NOME DO FICHEIRO: Ghost
public class Ghost : MonoBehaviour
{
    public float speed = 3f;
    private Transform target; // O Pac-Man
    private Rigidbody2D rb;

    [Tooltip("Arraste o _PacmanManager para aqui.")]
    public _PacmanManager gameManager; // <- MUDADO para _PacmanManager

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        // Tenta encontrar o Pac-Man sempre que o fantasma � ativado
        // � melhor do que no Start(), caso o Pac-Man ainda n�o exista
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }

    void FixedUpdate()
    {
        // Verifica se o alvo (target) existe E se o jogador est� vivo
        if (target == null || gameManager == null || !gameManager.playerController.gameObject.activeSelf)
        {
            rb.linearVelocity = Vector2.zero; // Para se o jogador for apanhado
            return;
        }

        // Calcula a dire��o para o jogador
        Vector2 directionToTarget = (target.position - transform.position);

        // IA Super Simples: Move-se no eixo (X ou Y) onde a dist�ncia � maior
        Vector2 newVelocity = Vector2.zero;
        if (Mathf.Abs(directionToTarget.x) > Mathf.Abs(directionToTarget.y))
        {
            // Move-se na horizontal
            newVelocity = new Vector2(Mathf.Sign(directionToTarget.x), 0);
        }
        else
        {
            // Move-se na vertical
            newVelocity = new Vector2(0, Mathf.Sign(directionToTarget.y));
        }

        rb.linearVelocity = newVelocity * speed;
    }

    /// <summary>
    /// Verifica se colidiu com o Pac-Man
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Informa o GameManager que o jogador morreu
            if (gameManager != null)
            {
                gameManager.PlayerCaught();
            }
        }
    }
}