using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Parámetros de Movimiento")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    [Header("Sensación de Salto (Game Feel)")]
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;

    [Header("Detección de Suelo")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Configuración de Audio SFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpSound;

    // Componentes
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;

    // Estado interno
    private float horizontalInput;
    private bool isGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        if (playerCollider != null && playerCollider.sharedMaterial == null)
        {
            PhysicsMaterial2D noFriction = new PhysicsMaterial2D("NoFriction");
            noFriction.friction = 0f;
            noFriction.bounciness = 0f;
            playerCollider.sharedMaterial = noFriction;
        }
    }

    private void Update()
    {
        // 1. Leer movimiento horizontal
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 2. Girar sprite
        if (horizontalInput > 0) spriteRenderer.flipX = false;
        else if (horizontalInput < 0) spriteRenderer.flipX = true;

        // 3. Chequear suelo
        CheckGrounded();

        // Coyote Time
        if (isGrounded) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;

        // Jump Buffer
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump")) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        // 4. Ejecutar Salto
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
            
            if (audioSource != null && jumpSound != null)
                audioSource.PlayOneShot(jumpSound);
        }

        // 5. Animación de plantar
        if (Input.GetKeyDown(KeyCode.E) && animator != null)
        {
            if (Mathf.Abs(rb.linearVelocity.x) < 0.1f && isGrounded)
                animator.SetTrigger("Plant");
        }

        // 6. Enviar parámetros al Animator
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
            animator.SetBool("isGrounded", isGrounded);
        }
    }

    private void FixedUpdate()
    {
        // Aplicar velocidad horizontal
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        // Modificadores de gravedad (estilo Mario)
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space) && !Input.GetButton("Jump"))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    private void CheckGrounded()
    {
        if (playerCollider == null)
        {
            isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.05f;
            return;
        }

        // Tamaño y posición de la caja ubicados justo debajo de los pies
        Vector2 boxSize = new Vector2(playerCollider.bounds.size.x * 0.8f, 0.05f);
        Vector2 boxCenter = new Vector2(playerCollider.bounds.center.x, playerCollider.bounds.min.y - (boxSize.y / 2f));

        LayerMask mask = groundLayer != 0 ? groundLayer : Physics2D.DefaultRaycastLayers;

        // OverlapBox detecta colisión bajo los pies sin problemas de origen dentro del collider
        Collider2D hit = Physics2D.OverlapBox(boxCenter, boxSize, 0f, mask);
        
        isGrounded = hit != null && hit != playerCollider && !hit.isTrigger;
    }

    private void OnDrawGizmosSelected()
    {
        if (playerCollider != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector2 boxSize = new Vector2(playerCollider.bounds.size.x * 0.8f, 0.05f);
            Vector2 boxCenter = new Vector2(playerCollider.bounds.center.x, playerCollider.bounds.min.y - (boxSize.y / 2f));
            Gizmos.DrawWireCube(boxCenter, boxSize);
        }
    }
}