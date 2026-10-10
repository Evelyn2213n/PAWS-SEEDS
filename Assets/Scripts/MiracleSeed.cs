using UnityEngine;
using System.Collections;
using TMPro; // Asegúrate de tener TextMeshPro importado en el proyecto

/// <summary>
/// Script para recolectar la Semilla Milagrosa.
/// Muestra un mensaje en pantalla al recolectarla.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MiracleSeed : MonoBehaviour
{
    [Header("UI Message")]
    [Tooltip("El texto de la UI donde se mostrará el mensaje.")]
    [SerializeField] private TextMeshProUGUI messageText;
    
    [Tooltip("Tiempo que el mensaje permanece en pantalla.")]
    [SerializeField] private float messageDuration = 3f;

    [Header("Efectos")]
    [Tooltip("Sonido opcional al recoger la semilla milagrosa.")]
    [SerializeField] private AudioClip pickupSound;

    private bool isCollected = false;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();
            if (inventory == null)
            {
                inventory = FindAnyObjectByType<PlayerInventory>();
            }

            if (inventory != null)
            {
                inventory.hasMiracleSeed = true;
                isCollected = true;

                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }

                // Iniciar la corrutina para mostrar el mensaje
                if (messageText != null)
                {
                    StartCoroutine(ShowMessageRoutine());
                }

                // Ocultar el sprite y deshabilitar colisiones mientras se muestra el mensaje
                GetComponent<SpriteRenderer>().enabled = false;
                GetComponent<Collider2D>().enabled = false;
                
                // Si no hay mensaje que mostrar, destruimos inmediatamente
                if (messageText == null)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    private IEnumerator ShowMessageRoutine()
    {
        messageText.text = "¡Obtuviste la Semilla Milagrosa!";
        messageText.gameObject.SetActive(true);

        yield return new WaitForSeconds(messageDuration);

        messageText.gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
