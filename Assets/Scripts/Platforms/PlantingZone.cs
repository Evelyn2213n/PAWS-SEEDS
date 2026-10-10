using UnityEngine;
using UnityEngine.Events;

namespace PawsAndSeeds.Platforms
{
    /// <summary>
    /// Zona o parcela de tierra interactiva donde Mack puede plantar una semilla
    /// para hacer crecer una enredadera, puente o escalera de plataformas.
    /// Muestra un indicador visual interactivo cuando Mack se acerca.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PlantingZone : MonoBehaviour
    {
        [Header("--- Planta / Enredadera a Hacer Crecer ---")]
        [Tooltip("Referencia a la enredadera/puente de plataformas que crecerá al sembrar.")]
        [SerializeField] private GrowablePlantPlatform plantToGrow;

        [Tooltip("Prefab alternativo en caso de no tener la planta instanciada previamente en la escena.")]
        [SerializeField] private GameObject plantPrefabToSpawn;

        [Tooltip("Punto donde se instanciará la planta si se usa el Prefab.")]
        [SerializeField] private Transform spawnPoint;

        [Header("--- Indicador Visual Interactivo ---")]
        [Tooltip("Objeto visual (ej. icono de semilla, tecla 'E' o brote seco) que se muestra cuando Mack entra.")]
        [SerializeField] private GameObject interactionPrompt;

        [Tooltip("Sprite opcional de brote seco que se oculta o transforma al plantar.")]
        [SerializeField] private SpriteRenderer drySproutRenderer;

        [Header("--- Ajustes de Control ---")]
        [Tooltip("Tecla principal de interacción para plantar.")]
        [SerializeField] private KeyCode plantKey = KeyCode.E;

        [Tooltip("Tecla secundaria de interacción.")]
        [SerializeField] private KeyCode secondaryPlantKey = KeyCode.J;

        [Header("--- Eventos Adicionales ---")]
        public UnityEvent onPlantSuccess;

        // Estado interno
        private bool isPlayerInside = false;
        private bool isPlanted = false;
        private PlayerInventory cachedPlayerInventory;
        private Animator cachedPlayerAnimator;

        public bool IsPlanted => isPlanted;

        private void Awake()
        {
            // Asegurar que el collider sea Trigger
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }

        private void Update()
        {
            if (isPlanted || !isPlayerInside) return;

            // Detectar pulsación de tecla para plantar
            if (Input.GetKeyDown(plantKey) || Input.GetKeyDown(secondaryPlantKey))
            {
                TryPlantSeed();
            }
        }

        /// <summary>
        /// Intenta plantar la semilla consumiendo el inventario y disparando el crecimiento.
        /// </summary>
        public bool TryPlantSeed()
        {
            if (isPlanted) return false;

            // Verificar inventario
            if (cachedPlayerInventory == null)
            {
                cachedPlayerInventory = FindAnyObjectByType<PlayerInventory>();
            }

            if (cachedPlayerInventory != null && !cachedPlayerInventory.UseSeed())
            {
                Debug.LogWarning("[PlantingZone] ¡Mack no tiene suficientes semillas!");
                return false;
            }

            // Activar animación en Mack
            if (cachedPlayerAnimator != null)
            {
                cachedPlayerAnimator.SetTrigger("Plant");
            }

            isPlanted = true;

            // Ocultar prompt de interacción
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }

            // Ocultar brote seco si existía
            if (drySproutRenderer != null)
            {
                drySproutRenderer.enabled = false;
            }

            // Iniciar crecimiento de la planta
            if (plantToGrow != null)
            {
                plantToGrow.GrowPlant();
            }
            else if (plantPrefabToSpawn != null)
            {
                Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
                GameObject spawned = Instantiate(plantPrefabToSpawn, spawnPos, Quaternion.identity);
                var growable = spawned.GetComponent<GrowablePlantPlatform>();
                if (growable != null)
                {
                    growable.GrowPlant();
                }
            }

            onPlantSuccess?.Invoke();
            return true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isPlanted) return;

            if (IsPlayer(other))
            {
                isPlayerInside = true;
                cachedPlayerInventory = other.GetComponent<PlayerInventory>();
                cachedPlayerAnimator = other.GetComponent<Animator>();

                if (cachedPlayerInventory == null)
                {
                    cachedPlayerInventory = other.GetComponentInParent<PlayerInventory>();
                }
                if (cachedPlayerAnimator == null)
                {
                    cachedPlayerAnimator = other.GetComponentInParent<Animator>();
                }

                if (interactionPrompt != null)
                {
                    interactionPrompt.SetActive(true);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (IsPlayer(other))
            {
                isPlayerInside = false;

                if (interactionPrompt != null)
                {
                    interactionPrompt.SetActive(false);
                }
            }
        }

        private bool IsPlayer(Collider2D col)
        {
            return col.CompareTag("Player") ||
                   col.GetComponent<PlayerController>() != null ||
                   col.GetComponentInParent<PlayerController>() != null ||
                   col.gameObject.layer == LayerMask.NameToLayer("Player");
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = isPlanted ? Color.gray : Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(1.5f, 1f, 0f));

            if (plantToGrow != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, plantToGrow.transform.position);
            }
        }
    }
}
