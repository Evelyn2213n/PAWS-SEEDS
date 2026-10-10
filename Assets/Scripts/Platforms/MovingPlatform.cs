using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PawsAndSeeds.Platforms
{
    /// <summary>
    /// Controlador de plataformas móviles para Unity 6 (Paws & Seeds).
    /// Compatible con movimiento horizontal, vertical y waypoints personalizados.
    /// Soporta activación al pisar, espera en extremos y transporte suave de pasajeros (Mack).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class MovingPlatform : MonoBehaviour
    {
        public enum MovementType
        {
            Horizontal,
            Vertical,
            CustomWaypoints
        }

        [Header("--- Modo de Movimiento ---")]
        [Tooltip("Tipo de trayectoria que seguirá la plataforma.")]
        [SerializeField] private MovementType movementType = MovementType.Horizontal;

        [Tooltip("Distancia que recorrerá en modo Horizontal o Vertical desde su punto de inicio.")]
        [SerializeField] private float travelDistance = 4f;

        [Tooltip("Waypoints personalizados en el mundo o relativos. Solo aplica en modo CustomWaypoints.")]
        [SerializeField] private List<Transform> customWaypoints = new List<Transform>();

        [Header("--- Parámetros de Velocidad y Espera ---")]
        [Tooltip("Velocidad de traslación de la plataforma.")]
        [SerializeField] private float moveSpeed = 2.5f;

        [Tooltip("Tiempo de pausa en segundos al llegar a cada extremo/waypoint.")]
        [SerializeField] private float waitTimeAtEnds = 0.6f;

        [Header("--- Activación e Interacción ---")]
        [Tooltip("Si es true, la plataforma permanecerá quieta hasta que Mack se pare sobre ella.")]
        [SerializeField] private bool activateOnTouch = false;

        [Tooltip("Si es true, realizará el recorrido una sola vez y se detendrá al llegar al final.")]
        [SerializeField] private bool moveOnlyOnce = false;

        [Tooltip("Filtro opcional para detectar al jugador por Layer. Si es Nothing, detecta PlayerController por componente.")]
        [SerializeField] private LayerMask playerLayerMask;

        [Header("--- Suavizado de Bordes (Anti-Stuck) ---")]
        [Tooltip("Radio de curvatura en las esquinas del BoxCollider2D para evitar que el jugador se trabe.")]
        [Range(0.01f, 0.15f)]
        [SerializeField] private float colliderEdgeRadius = 0.05f;

        // Referencias internas
        private Rigidbody2D rb;
        private BoxCollider2D boxCollider;
        private List<Vector2> waypoints = new List<Vector2>();
        private int currentTargetIndex = 1;
        private int direction = 1; // 1 = adelante, -1 = reversa
        private bool isWaiting = false;
        private bool isActivated = false;
        private bool hasCompletedOnce = false;
        private Vector2 previousPosition;

        // Pasajeros sobre la plataforma
        private readonly HashSet<Rigidbody2D> passengers = new HashSet<Rigidbody2D>();

        public bool IsActivated => isActivated;
        public Vector2 CurrentVelocity => rb != null ? rb.linearVelocity : Vector2.zero;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            boxCollider = GetComponent<BoxCollider2D>();

            // Configuración física para Unity 6
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.useFullKinematicContacts = true;

            // Suavizar las esquinas del collider para evitar trabas mecánicas
            if (boxCollider != null && boxCollider.edgeRadius < colliderEdgeRadius)
            {
                boxCollider.edgeRadius = colliderEdgeRadius;
            }

            SetupWaypoints();
        }

        private void Start()
        {
            previousPosition = rb.position;
            // Si no requiere activación por contacto, arranca activada
            if (!activateOnTouch)
            {
                isActivated = true;
            }
        }

        /// <summary>
        /// Inicializa los puntos de destino según el modo configurado.
        /// </summary>
        public void SetupWaypoints()
        {
            waypoints.Clear();
            Vector2 origin = transform.position;
            waypoints.Add(origin);

            switch (movementType)
            {
                case MovementType.Horizontal:
                    waypoints.Add(origin + Vector2.right * travelDistance);
                    break;

                case MovementType.Vertical:
                    waypoints.Add(origin + Vector2.up * travelDistance);
                    break;

                case MovementType.CustomWaypoints:
                    if (customWaypoints != null && customWaypoints.Count > 0)
                    {
                        waypoints.Clear();
                        foreach (var wp in customWaypoints)
                        {
                            if (wp != null) waypoints.Add(wp.position);
                        }
                    }
                    else
                    {
                        // Fallback a posición actual si la lista está vacía
                        waypoints.Add(origin + Vector2.right * travelDistance);
                    }
                    break;
            }

            currentTargetIndex = (waypoints.Count > 1) ? 1 : 0;
        }

        private void FixedUpdate()
        {
            Vector2 currentPosition = rb.position;

            // Si está esperando, desactivada o completó su ciclo único
            if (isWaiting || !isActivated || (moveOnlyOnce && hasCompletedOnce) || waypoints.Count < 2)
            {
                rb.linearVelocity = Vector2.zero;
                MovePassengers(Vector2.zero);
                previousPosition = currentPosition;
                return;
            }

            Vector2 targetPoint = waypoints[currentTargetIndex];
            Vector2 directionToTarget = targetPoint - currentPosition;
            float distanceToTarget = directionToTarget.magnitude;

            // Verificar si alcanzó el punto de destino
            if (distanceToTarget <= 0.05f)
            {
                rb.position = targetPoint;
                rb.linearVelocity = Vector2.zero;
                StartCoroutine(WaitAndSwitchTargetRoutine());
            }
            else
            {
                Vector2 moveDirection = directionToTarget.normalized;
                Vector2 targetVelocity = moveDirection * moveSpeed;

                // Unity 6: Uso de linearVelocity en Rigidbody2D Kinemático
                rb.linearVelocity = targetVelocity;
            }

            // Desplazamiento delta físico para arrastrar pasajeros sin alterar su escala de jerarquía
            Vector2 deltaMovement = rb.position - previousPosition;
            MovePassengers(deltaMovement);

            previousPosition = rb.position;
        }

        /// <summary>
        /// Mueve a los personajes parados sobre la plataforma mediante física,
        /// evitando desajustes de escala y tirones de velocidad horizontal.
        /// </summary>
        private void MovePassengers(Vector2 deltaMovement)
        {
            if (passengers.Count == 0 || deltaMovement == Vector2.zero) return;

            foreach (var passengerRb in passengers)
            {
                if (passengerRb != null)
                {
                    // Desplazamos la posición física del pasajero exactamente con la plataforma
                    passengerRb.position += deltaMovement;
                }
            }
        }

        private IEnumerator WaitAndSwitchTargetRoutine()
        {
            isWaiting = true;
            yield return new WaitForSeconds(waitTimeAtEnds);
            isWaiting = false;

            // Lógica de avance de waypoints (Ping-Pong)
            if (movementType == MovementType.CustomWaypoints && customWaypoints.Count > 2)
            {
                currentTargetIndex += direction;

                if (currentTargetIndex >= waypoints.Count)
                {
                    if (moveOnlyOnce)
                    {
                        hasCompletedOnce = true;
                        yield break;
                    }
                    direction = -1;
                    currentTargetIndex = waypoints.Count - 2;
                }
                else if (currentTargetIndex < 0)
                {
                    direction = 1;
                    currentTargetIndex = 1;
                }
            }
            else
            {
                // Ping-pong estándar entre punto A (0) y punto B (1)
                currentTargetIndex = (currentTargetIndex == 0) ? 1 : 0;
                if (currentTargetIndex == 0 && moveOnlyOnce)
                {
                    hasCompletedOnce = true;
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Solo considerar al personaje si aterriza desde arriba (contacto superior)
            if (IsPlayerLandingOnTop(collision))
            {
                Rigidbody2D playerRb = collision.rigidbody;
                if (playerRb != null)
                {
                    passengers.Add(playerRb);
                }

                // Activar si está configurada para iniciar al contacto
                if (activateOnTouch && !isActivated)
                {
                    isActivated = true;
                }
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            Rigidbody2D playerRb = collision.rigidbody;
            if (playerRb != null && passengers.Contains(playerRb))
            {
                passengers.Remove(playerRb);
            }
        }

        /// <summary>
        /// Comprueba si el contacto se dio desde la parte superior de la plataforma
        /// y si el objeto corresponde al jugador.
        /// </summary>
        private bool IsPlayerLandingOnTop(Collision2D collision)
        {
            // Verificar Layer o componente PlayerController
            bool isPlayer = false;
            if (playerLayerMask != 0)
            {
                isPlayer = ((1 << collision.gameObject.layer) & playerLayerMask) != 0;
            }
            else
            {
                isPlayer = collision.gameObject.CompareTag("Player") ||
                           collision.gameObject.GetComponent<PlayerController>() != null;
            }

            if (!isPlayer) return false;

            // Normal de contacto: si el jugador desciende, la normal apunta hacia arriba respecto a la plataforma
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Normal.y < -0.5f significa que el otro objeto está situado encima de nosotros
                if (contact.normal.y < -0.5f)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnDisable()
        {
            passengers.Clear();
            isWaiting = false;
        }

        // --- Visualización de Trayectorias en el Editor ---
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector2 start = Application.isPlaying && waypoints.Count > 0 ? waypoints[0] : (Vector2)transform.position;

            if (movementType == MovementType.Horizontal)
            {
                Vector2 end = start + Vector2.right * travelDistance;
                Gizmos.DrawLine(start, end);
                Gizmos.DrawWireSphere(start, 0.2f);
                Gizmos.DrawWireSphere(end, 0.2f);
            }
            else if (movementType == MovementType.Vertical)
            {
                Vector2 end = start + Vector2.up * travelDistance;
                Gizmos.DrawLine(start, end);
                Gizmos.DrawWireSphere(start, 0.2f);
                Gizmos.DrawWireSphere(end, 0.2f);
            }
            else if (movementType == MovementType.CustomWaypoints && customWaypoints != null)
            {
                for (int i = 0; i < customWaypoints.Count; i++)
                {
                    if (customWaypoints[i] != null)
                    {
                        Gizmos.DrawWireSphere(customWaypoints[i].position, 0.25f);
                        if (i < customWaypoints.Count - 1 && customWaypoints[i + 1] != null)
                        {
                            Gizmos.DrawLine(customWaypoints[i].position, customWaypoints[i + 1].position);
                        }
                    }
                }
            }
        }
    }
}
