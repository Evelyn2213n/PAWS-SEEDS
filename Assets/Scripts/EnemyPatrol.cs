using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Parámetros de Patrullaje")]
    [Tooltip("Velocidad de movimiento del robot.")]
    [SerializeField] private float speed = 2f;

    [Tooltip("Distancia que recorrerá a la izquierda y derecha desde su posición inicial.")]
    [SerializeField] private float patrolRange = 3f;

    [Tooltip("Tiempo de espera en segundos antes de girar al llegar al límite.")]
    [SerializeField] private float waitTimeAtTurn = 0.5f;

    [Header("Orientación del Sprite")]
    [Tooltip("¿El sprite original mira hacia la derecha?")]
    [SerializeField] private bool spriteFacesRight = true;

    [Header("Daño al Jugador (Opcional)")]
    [Tooltip("Cantidad de daño infligido al jugador al colisionar.")]
    [SerializeField] private int damageAmount = 1;

    [Tooltip("Fuerza de empuje aplicada al jugador al hacer daño.")]
    [SerializeField] private float knockbackForce = 5f;

    // Variables internas
    private Vector2 startPosition;
    private float leftBound;
    private float rightBound;
    private bool movingRight = true;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    // Componentes
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        startPosition = transform.position;
        leftBound = startPosition.x - patrolRange;
        rightBound = startPosition.x + patrolRange;

        UpdateSpriteFacing();
    }

    private void Update()
    {
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                movingRight = !movingRight;
                UpdateSpriteFacing();
            }
        }
    }

    private void FixedUpdate()
    {
        if (isWaiting)
        {
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
            return;
        }

        Move();
    }

    private void Move()
    {
        float direction = movingRight ? 1f : -1f;

        // Comprobar si alcanzó los límites de patrullaje
        if (movingRight && transform.position.x >= rightBound)
        {
            StartWaiting();
            return;
        }
        else if (!movingRight && transform.position.x <= leftBound)
        {
            StartWaiting();
            return;
        }

        // Aplicar movimiento
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
        }
        else
        {
            transform.Translate(Vector2.right * direction * speed * Time.fixedDeltaTime);
        }
    }

    private void StartWaiting()
    {
        isWaiting = true;
        waitTimer = waitTimeAtTurn;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    private void UpdateSpriteFacing()
    {
        if (spriteRenderer != null)
        {
            // Si el sprite base mira a la derecha:
            // - Moviéndose a la derecha -> flipX = false
            // - Moviéndose a la izquierda -> flipX = true
            spriteRenderer.flipX = spriteFacesRight ? !movingRight : movingRight;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerDamage(collision.gameObject, collision.contacts.Length > 0 ? collision.contacts[0].point : (Vector2)transform.position);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandlePlayerDamage(other.gameObject, other.transform.position);
    }

    private void HandlePlayerDamage(GameObject target, Vector2 contactPoint)
    {
        if (target.CompareTag("Player"))
        {
            HealthManager health = FindAnyObjectByType<HealthManager>();
            if (health != null)
            {
                health.TakeDamage(damageAmount);
            }

            // Aplicar retroceso al jugador
            Rigidbody2D playerRb = target.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 knockbackDir = ((Vector2)target.transform.position - (Vector2)transform.position).normalized;
                knockbackDir.y = 0.5f; // Un pequeño empuje hacia arriba
                playerRb.linearVelocity = Vector2.zero;
                playerRb.AddForce(knockbackDir.normalized * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Dibuja el rango de patrulla en la escena de Unity para facilitar la configuración visual
        Gizmos.color = Color.red;
        Vector2 center = Application.isPlaying ? startPosition : (Vector2)transform.position;
        Vector3 left = new Vector3(center.x - patrolRange, center.y, transform.position.z);
        Vector3 right = new Vector3(center.x + patrolRange, center.y, transform.position.z);

        Gizmos.DrawLine(left, right);
        Gizmos.DrawWireSphere(left, 0.2f);
        Gizmos.DrawWireSphere(right, 0.2f);
    }
}
