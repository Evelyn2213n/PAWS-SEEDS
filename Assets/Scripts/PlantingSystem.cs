using UnityEngine;
using PawsAndSeeds.Platforms;

/// <summary>
/// Sistema de plantación integrado para Mack (Paws & Seeds).
/// Permite plantar en zonas clave (PlantingZone) para puentes y escaleras,
/// o en el suelo libre para crear plataformas portátiles donde escalar.
/// </summary>
public class PlantingSystem : MonoBehaviour
{
    [Header("--- Teclas de Plantación ---")]
    [Tooltip("Tecla principal de plantado (coincide con la animación 'E' de Mack).")]
    [SerializeField] private KeyCode plantKey = KeyCode.E;

    [Tooltip("Tecla alternativa de plantado.")]
    [SerializeField] private KeyCode secondaryKey = KeyCode.J;

    [Header("--- Configuración de Plantas Libres ---")]
    [Tooltip("Prefab de la planta o flor plataforma que aparecerá frente a Mack.")]
    public GameObject plantPrefab;

    [Tooltip("Punto de spawn frente a Mack donde aparecerá la planta.")]
    public Transform plantPoint;

    [Tooltip("Distancia hacia el suelo para alinear la planta.")]
    [SerializeField] private LayerMask groundLayer;

    // Componentes del jugador
    private PlayerInventory inventory;
    private Animator animator;
    private Rigidbody2D rb;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(plantKey) || Input.GetKeyDown(secondaryKey))
        {
            HandlePlantInput();
        }
    }

    private void HandlePlantInput()
    {
        // 1. Verificar si Mack está dentro de una PlantingZone específica (prioridad al puzzle/abismo)
        Collider2D[] nearbyTriggers = Physics2D.OverlapCircleAll(transform.position, 1.2f);
        foreach (var col in nearbyTriggers)
        {
            if (col.isTrigger)
            {
                var zone = col.GetComponent<PlantingZone>();
                if (zone != null && !zone.IsPlanted)
                {
                    // La PlantingZone se encarga de consumir semilla y hacer crecer el puente
                    zone.TryPlantSeed();
                    return;
                }
            }
        }

        // 2. Si no hay PlantingZone, plantar una plataforma libre en el suelo
        TryPlantFreeform();
    }

    /// <summary>
    /// Planta una flor o plataforma libre frente a Mack si tiene semillas y suelo sólido.
    /// </summary>
    public void TryPlantFreeform()
    {
        if (plantPrefab == null)
        {
            Debug.LogWarning("[PlantingSystem] No hay plantPrefab asignado en el Inspector.");
            return;
        }

        // Verificar que Mack tenga semillas
        if (inventory == null || inventory.seedCount <= 0)
        {
            Debug.Log("[PlantingSystem] ¡Sin semillas en el inventario!");
            return;
        }

        // Posición de spawn (frente al jugador o ajustada al suelo)
        Vector3 spawnPosition = plantPoint != null ? plantPoint.position : transform.position + (transform.right * 0.8f);

        // Raycast opcional para ajustar la planta al suelo exacto
        RaycastHit2D hit = Physics2D.Raycast(spawnPosition + Vector3.up * 0.5f, Vector2.down, 2f, groundLayer != 0 ? groundLayer : Physics2D.AllLayers);
        if (hit.collider != null && !hit.collider.isTrigger)
        {
            spawnPosition.y = hit.point.y;
        }

        // Consumir semilla
        if (inventory.UseSeed())
        {
            // Ejecutar animación en Mack
            if (animator != null)
            {
                animator.SetTrigger("Plant");
            }

            // Instanciar planta
            GameObject plantInstance = Instantiate(plantPrefab, spawnPosition, Quaternion.identity);

            // Activar crecimiento si tiene GrowablePlantPlatform o PlantGrowth
            var growable = plantInstance.GetComponent<GrowablePlantPlatform>();
            if (growable != null)
            {
                growable.GrowPlant();
            }
            else
            {
                var legacyGrowth = plantInstance.GetComponent<PlantGrowth>();
                if (legacyGrowth == null)
                {
                    // Si no tiene ningún script, asegurar que su collider tenga layer Ground
                    plantInstance.layer = LayerMask.NameToLayer("Ground");
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (plantPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(plantPoint.position, 0.2f);
        }
    }
}