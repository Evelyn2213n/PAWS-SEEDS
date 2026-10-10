using System.Collections;
using UnityEngine;

/// <summary>
/// Control de crecimiento por etapas de flores y brotes individuales para Paws & Seeds.
/// Convierte automáticamente la flor final en una plataforma sólida con capa 'Ground' y bordes redondeados.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlantGrowth : MonoBehaviour
{
    [Header("--- Sprites por Etapa ---")]
    [Tooltip("Etapa 1: Semilla o suelo árido")]
    public Sprite aridSprite;

    [Tooltip("Etapa 2: Brote o tallo verde")]
    public Sprite sproutSprite;

    [Tooltip("Etapa 3: Flor o plataforma completa")]
    public Sprite flowerSprite;

    [Header("--- Parámetros de Crecimiento ---")]
    [Tooltip("Tiempo de espera entre cada etapa (en segundos).")]
    public float growthInterval = 0.8f;

    [Tooltip("Si es true, la flor da un pequeño rebote visual al florecer.")]
    [SerializeField] private bool bounceOnBloom = true;

    [Tooltip("Material físico sin fricción opcional para evitar atascos.")]
    [SerializeField] private PhysicsMaterial2D platformPhysicsMaterial;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D plantCollider;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        plantCollider = GetComponent<BoxCollider2D>();

        // Configurar capa Ground automáticamente
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer != -1)
        {
            gameObject.layer = groundLayer;
        }

        if (plantCollider != null)
        {
            plantCollider.enabled = false; // Aún no es plataforma sólida
            plantCollider.edgeRadius = 0.04f; // Bordes redondeados anti-stuck

            if (platformPhysicsMaterial != null)
            {
                plantCollider.sharedMaterial = platformPhysicsMaterial;
            }
        }
    }

    private void Start()
    {
        StartCoroutine(GrowProcess());
    }

    private IEnumerator GrowProcess()
    {
        // Etapa 1: Árido / Semilla
        if (aridSprite != null) spriteRenderer.sprite = aridSprite;
        yield return new WaitForSeconds(growthInterval);

        // Etapa 2: Brote
        if (sproutSprite != null) spriteRenderer.sprite = sproutSprite;
        yield return new WaitForSeconds(growthInterval);

        // Etapa 3: Flor completa (Plataforma activa)
        if (flowerSprite != null) spriteRenderer.sprite = flowerSprite;

        // Rebote visual de florecimiento (Game Feel)
        if (bounceOnBloom)
        {
            Vector3 originalScale = transform.localScale;
            transform.localScale = originalScale * 0.7f;

            float elapsed = 0f;
            float bounceDuration = 0.3f;
            while (elapsed < bounceDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / bounceDuration;
                // Efecto elástico Overshoot
                float scaleMultiplier = Mathf.LerpUnclamped(0.7f, 1.15f, Mathf.Sin(t * Mathf.PI * 0.5f));
                if (t > 0.7f)
                {
                    scaleMultiplier = Mathf.Lerp(1.15f, 1f, (t - 0.7f) / 0.3f);
                }
                transform.localScale = originalScale * scaleMultiplier;
                yield return null;
            }
            transform.localScale = originalScale;
        }

        // Habilitar plataforma sólida para que Mack se pare en ella
        if (plantCollider != null)
        {
            plantCollider.enabled = true;
        }
    }
}