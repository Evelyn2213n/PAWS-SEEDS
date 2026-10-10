using UnityEngine;

/// <summary>
/// Objeto recolectable de semilla en el mundo del juego.
/// Cuando Mack pasa por encima, suma semillas a su inventario.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SeedCollectible : MonoBehaviour
{
    [Header("--- Propiedades de la Semilla ---")]
    [Tooltip("Cantidad de semillas que otorga este ítem.")]
    [SerializeField] private int seedAmount = 1;

    [Header("--- Animación Flotante Opcional ---")]
    [Tooltip("¿Hacer que la semilla flote suavemente arriba y abajo?")]
    [SerializeField] private bool enableFloating = true;
    [SerializeField] private float floatSpeed = 2.5f;
    [SerializeField] private float floatAmplitude = 0.15f;

    [Header("--- Efectos Opcionales ---")]
    [Tooltip("Sonido que se reproduce al recoger la semilla.")]
    [SerializeField] private AudioClip pickupSound;

    private Vector3 startPos;
    private bool isCollected = false;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        if (enableFloating && !isCollected)
        {
            float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(startPos.x, newY, startPos.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player") || other.GetComponent<PlayerInventory>() != null)
        {
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();
            if (inventory == null)
            {
                inventory = FindAnyObjectByType<PlayerInventory>();
            }

            if (inventory != null)
            {
                bool added = inventory.AddSeed(seedAmount);
                if (added)
                {
                    isCollected = true;

                    // Reproducir sonido si hay clip disponible
                    if (pickupSound != null)
                    {
                        AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                    }

                    Destroy(gameObject);
                }
            }
        }
    }
}
