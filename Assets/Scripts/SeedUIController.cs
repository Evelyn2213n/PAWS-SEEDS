using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla la interfaz gráfica (UI) del contador y la barra de semillas de Mack.
/// Se actualiza automáticamente cuando el jugador recolecta o gasta semillas.
/// </summary>
public class SeedUIController : MonoBehaviour
{
    [Header("--- Referencias de Componentes UI ---")]
    [Tooltip("Imagen que muestra el icono de la semilla.")]
    [SerializeField] private Image seedIcon;

    [Tooltip("Texto numérico que muestra el contador de semillas (ej: '5 / 10').")]
    [SerializeField] private TextMeshProUGUI counterText;

    [Tooltip("Barra de relleno que muestra el progreso/capacidad de semillas.")]
    [SerializeField] private Image fillBar;

    [Tooltip("Slider alternativo si se usa un Slider de Unity.")]
    [SerializeField] private Slider seedSlider;

    [Header("--- Personalización de Texto ---")]
    [Tooltip("Formato del texto: {0} = actuales, {1} = máximas.")]
    [SerializeField] private string textFormat = "{0} / {1}";

    [Header("--- Animación Suave (Opcional) ---")]
    [Tooltip("¿Animar suavemente la barra de llenado?")]
    [SerializeField] private bool smoothFill = true;
    [SerializeField] private float fillSpeed = 6f;

    [Header("--- Referencia al Jugador ---")]
    [SerializeField] private PlayerInventory playerInventory;

    private float targetFill = 0f;
    private float currentFill = 0f;

    private void Awake()
    {
        if (playerInventory == null)
        {
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        }
    }

    private void OnEnable()
    {
        if (playerInventory == null)
        {
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        }

        if (playerInventory != null)
        {
            playerInventory.OnSeedsChanged += UpdateSeedsUI;
            // Actualización inicial
            UpdateSeedsUI(playerInventory.seedCount, playerInventory.maxSeeds);
        }
    }

    private void OnDisable()
    {
        if (playerInventory != null)
        {
            playerInventory.OnSeedsChanged -= UpdateSeedsUI;
        }
    }

    private void Start()
    {
        if (playerInventory != null)
        {
            UpdateSeedsUI(playerInventory.seedCount, playerInventory.maxSeeds);
        }
    }

    private void Update()
    {
        if (smoothFill && fillBar != null)
        {
            currentFill = Mathf.MoveTowards(currentFill, targetFill, fillSpeed * Time.deltaTime);
            fillBar.fillAmount = currentFill;
        }
    }

    /// <summary>
    /// Actualiza la barra y el texto del contador de semillas.
    /// </summary>
    public void UpdateSeedsUI(int current, int max)
    {
        float ratio = max > 0 ? (float)current / max : 0f;
        targetFill = Mathf.Clamp01(ratio);

        if (!smoothFill && fillBar != null)
        {
            fillBar.fillAmount = targetFill;
            currentFill = targetFill;
        }

        if (seedSlider != null)
        {
            seedSlider.maxValue = max;
            seedSlider.value = current;
        }

        if (counterText != null)
        {
            counterText.text = string.Format(textFormat, current, max);
        }
    }
}
