using UnityEngine;

/// <summary>
/// Gestiona el agujero donde se planta la Semilla Milagrosa.
/// Requiere que el jugador tenga la semilla y que el jefe esté derrotado.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MiracleHole : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El GameObject del sprite de la Cueva que aparecerá después de plantar.")]
    [SerializeField] private GameObject caveObject;
    
    [Tooltip("Animator del agujero para reproducir la animación de crecimiento/plantado (opcional).")]
    [SerializeField] private Animator holeAnimator;
    
    [Tooltip("Nombre del trigger en el Animator.")]
    [SerializeField] private string plantAnimationTrigger = "Plant";

    [Header("Configuración")]
    [Tooltip("Tecla para interactuar y plantar la semilla.")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    
    [Tooltip("Tiempo de retraso antes de mostrar la cueva después de plantar.")]
    [SerializeField] private float timeToRevealCave = 1.5f;

    private bool isPlanted = false;
    private bool playerInRange = false;
    private PlayerInventory playerInventory;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        if (caveObject != null)
        {
            caveObject.SetActive(false); // Ocultar cueva al inicio
        }
    }

    private void Update()
    {
        if (playerInRange && !isPlanted && Input.GetKeyDown(interactKey))
        {
            TryPlantMiracleSeed();
        }
    }

    private void TryPlantMiracleSeed()
    {
        if (playerInventory != null && playerInventory.hasMiracleSeed)
        {
            if (BossManager.isBossDefeated)
            {
                PlantSeed();
            }
            else
            {
                Debug.Log("[MiracleHole] ¡Aún no has derrotado al jefe! No puedes plantar esto todavía.");
            }
        }
        else
        {
            Debug.Log("[MiracleHole] Necesitas la Semilla Milagrosa para interactuar aquí.");
        }
    }

    private void PlantSeed()
    {
        isPlanted = true;
        playerInventory.hasMiracleSeed = false; // Consumimos la semilla milagrosa

        Debug.Log("[MiracleHole] ¡Semilla Milagrosa plantada con éxito!");

        if (holeAnimator != null)
        {
            holeAnimator.SetTrigger(plantAnimationTrigger);
        }

        // Retrasamos la aparición de la cueva para dejar que la animación se reproduzca
        Invoke(nameof(ShowCave), timeToRevealCave);
    }

    private void ShowCave()
    {
        if (caveObject != null)
        {
            caveObject.SetActive(true);
            Debug.Log("[MiracleHole] ¡La cueva ha aparecido! Lista para cambiar de nivel.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerInventory = other.GetComponent<PlayerInventory>();
            if (playerInventory == null)
            {
                playerInventory = FindAnyObjectByType<PlayerInventory>();
            }
            Debug.Log("[MiracleHole] Presiona '" + interactKey.ToString() + "' para plantar.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerInventory = null;
        }
    }
}
