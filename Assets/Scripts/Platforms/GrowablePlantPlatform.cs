using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PawsAndSeeds.Platforms
{
    /// <summary>
    /// Controla el crecimiento dinámico de plantas, enredaderas y flores que actúan como
    /// puentes o escaleras de plataformas para que Mack pueda subir o cruzar abismos.
    /// Soporta animación procedural con elasticidad (pop-up), secuencias escalonadas y bordes anti-stuck.
    /// </summary>
    public class GrowablePlantPlatform : MonoBehaviour
    {
        public enum GrowthPattern
        {
            SequentialSteps, // Cada escalón brota uno tras otro (ideal para puentes de lianas y escaleras)
            Simultaneous,    // Todas las partes crecen al mismo tiempo
            VerticalStem     // Crece un tallo central y luego florecen las plataformas laterales
        }

        [Serializable]
        public class PlantStep
        {
            [Tooltip("Nombre descriptivo del escalón o rama.")]
            public string stepName = "Escalon";

            [Tooltip("GameObject del escalón que contiene el sprite y collider.")]
            public GameObject stepObject;

            [Tooltip("Retraso individual antes de que empiece a brotar este escalón.")]
            public float delayBeforeGrow = 0.25f;

            [Tooltip("Escala objetivo final del escalón.")]
            public Vector3 targetScale = Vector3.one;

            [HideInInspector] public Collider2D stepCollider;
            [HideInInspector] public SpriteRenderer stepRenderer;
        }

        [Header("--- Modo de Crecimiento ---")]
        [Tooltip("Patrón de aparición de las ramas/plataformas.")]
        [SerializeField] private GrowthPattern pattern = GrowthPattern.SequentialSteps;

        [Tooltip("Velocidad o duración del crecimiento de cada escalón (en segundos).")]
        [SerializeField] private float stepGrowDuration = 0.45f;

        [Tooltip("Si es true, la planta brota automáticamente al iniciar la escena.")]
        [SerializeField] private bool growOnStart = false;

        [Header("--- Escalones / Plataformas de la Planta ---")]
        [Tooltip("Lista de plataformas/hojas/ramas que componen el puente o escalera.")]
        [SerializeField] private List<PlantStep> plantSteps = new List<PlantStep>();

        [Header("--- Tallo / Enredadera Base (Opcional) ---")]
        [Tooltip("Objeto que representa la liana o tallo principal de fondo.")]
        [SerializeField] private GameObject vineStemObject;
        [SerializeField] private Vector3 vineStemTargetScale = Vector3.one;

        [Header("--- Material y Capa Física (Anti-Stuck) ---")]
        [Tooltip("Física sin fricción para evitar que Mack se trabe en esquinas.")]
        [SerializeField] private PhysicsMaterial2D platformPhysicsMaterial;

        [Tooltip("Capa de suelo para que PlayerController reconozca el suelo.")]
        [SerializeField] private string groundLayerName = "Ground";

        [Tooltip("Radio de suavizado de esquinas para los BoxCollider2D.")]
        [Range(0.01f, 0.15f)]
        [SerializeField] private float edgeRadius = 0.04f;

        [Header("--- Eventos y Efectos ---")]
        [Tooltip("Partículas opcionales al florecer o crecer.")]
        [SerializeField] private ParticleSystem growthParticles;

        [Tooltip("Sonido opcional de crecimiento.")]
        [SerializeField] private AudioSource growthAudio;

        // Estado
        public bool IsFullyGrown { get; private set; } = false;
        public bool IsGrowing { get; private set; } = false;

        private void Awake()
        {
            InitializeSteps();
        }

        private void Start()
        {
            if (growOnStart)
            {
                GrowPlant();
            }
        }

        /// <summary>
        /// Configura los colliders, capas y oculta las plataformas hasta que crezcan.
        /// </summary>
        public void InitializeSteps()
        {
            int groundLayer = LayerMask.NameToLayer(groundLayerName);
            if (groundLayer == -1) groundLayer = 0;

            // Inicializar tallo si existe
            if (vineStemObject != null)
            {
                if (vineStemTargetScale == Vector3.zero)
                {
                    vineStemTargetScale = vineStemObject.transform.localScale;
                }
                vineStemObject.transform.localScale = Vector3.zero;
            }

            // Inicializar cada escalón
            foreach (var step in plantSteps)
            {
                if (step.stepObject == null) continue;

                if (step.targetScale == Vector3.zero)
                {
                    step.targetScale = step.stepObject.transform.localScale;
                }

                // Ocultar al inicio
                step.stepObject.transform.localScale = Vector3.zero;

                // Configurar Collider y capa
                step.stepCollider = step.stepObject.GetComponent<Collider2D>();
                if (step.stepCollider != null)
                {
                    step.stepCollider.enabled = false;

                    // Si es BoxCollider2D, aplicar suavizado de bordes anti-atasco
                    if (step.stepCollider is BoxCollider2D box)
                    {
                        box.edgeRadius = edgeRadius;
                        if (platformPhysicsMaterial != null)
                        {
                            box.sharedMaterial = platformPhysicsMaterial;
                        }
                    }
                }

                // Asignar capa Ground
                step.stepObject.layer = groundLayer;
            }
        }

        /// <summary>
        /// Inicia el crecimiento de la planta. Puede ser llamado por PlantingZone o PlantingSystem.
        /// </summary>
        public void GrowPlant()
        {
            if (IsGrowing || IsFullyGrown) return;

            StartCoroutine(GrowthRoutine());
        }

        private IEnumerator GrowthRoutine()
        {
            IsGrowing = true;

            if (growthAudio != null)
            {
                growthAudio.Play();
            }

            if (growthParticles != null)
            {
                growthParticles.Play();
            }

            // 1. Desplegar el tallo principal si existe
            if (vineStemObject != null)
            {
                yield return StartCoroutine(AnimateScaleBounce(vineStemObject.transform, vineStemTargetScale, stepGrowDuration * 0.8f));
            }

            // 2. Desplegar los escalones de acuerdo al patrón
            if (pattern == GrowthPattern.SequentialSteps)
            {
                for (int i = 0; i < plantSteps.Count; i++)
                {
                    var step = plantSteps[i];
                    if (step.stepObject == null) continue;

                    if (step.delayBeforeGrow > 0f)
                    {
                        yield return new WaitForSeconds(step.delayBeforeGrow);
                    }

                    // Animar brote del escalón con pop elástico
                    yield return StartCoroutine(AnimateScaleBounce(step.stepObject.transform, step.targetScale, stepGrowDuration));

                    // Activar el collider para que sea una plataforma sólida
                    if (step.stepCollider != null)
                    {
                        step.stepCollider.enabled = true;
                    }
                }
            }
            else // Simultaneous o VerticalStem
            {
                List<Coroutine> activeCoroutines = new List<Coroutine>();

                foreach (var step in plantSteps)
                {
                    if (step.stepObject == null) continue;
                    activeCoroutines.Add(StartCoroutine(GrowSingleStepRoutine(step)));
                }

                foreach (var c in activeCoroutines)
                {
                    yield return c;
                }
            }

            IsGrowing = false;
            IsFullyGrown = true;
        }

        private IEnumerator GrowSingleStepRoutine(PlantStep step)
        {
            if (step.delayBeforeGrow > 0f)
            {
                yield return new WaitForSeconds(step.delayBeforeGrow);
            }

            yield return StartCoroutine(AnimateScaleBounce(step.stepObject.transform, step.targetScale, stepGrowDuration));

            if (step.stepCollider != null)
            {
                step.stepCollider.enabled = true;
            }
        }

        /// <summary>
        /// Animación procedural con efecto 'Pop & Elasticity' (squash & stretch) para un look jugoso.
        /// </summary>
        private IEnumerator AnimateScaleBounce(Transform target, Vector3 finalScale, float duration)
        {
            float elapsed = 0f;
            Vector3 startScale = Vector3.zero;

            // Escala exagerada para el rebote
            Vector3 overshootScale = new Vector3(
                finalScale.x * 1.18f,
                finalScale.y * 1.18f,
                finalScale.z
            );

            // Fase 1: Crecer de 0 a Overshoot (80% del tiempo)
            float phase1Duration = duration * 0.75f;
            while (elapsed < phase1Duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / phase1Duration);
                // Curva suave EaseOutQuad
                t = Mathf.Sin(t * Mathf.PI * 0.5f);
                target.localScale = Vector3.Lerp(startScale, overshootScale, t);
                yield return null;
            }

            // Fase 2: Asentarse de Overshoot a FinalScale con rebote elástico (25% restante)
            elapsed = 0f;
            float phase2Duration = duration * 0.25f;
            while (elapsed < phase2Duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / phase2Duration);
                target.localScale = Vector3.Lerp(overshootScale, finalScale, t);
                yield return null;
            }

            target.localScale = finalScale;
        }

        /// <summary>
        /// Permite resetear la planta (por ejemplo si se reinicia un puzzle).
        /// </summary>
        public void ResetPlant()
        {
            StopAllCoroutines();
            IsGrowing = false;
            IsFullyGrown = false;
            InitializeSteps();
        }

        /// <summary>
        /// Método de conveniencia para añadir escalones rápidamente por código o editor.
        /// </summary>
        public void AddStep(GameObject stepObj, float delay = 0.2f)
        {
            plantSteps.Add(new PlantStep
            {
                stepName = stepObj.name,
                stepObject = stepObj,
                delayBeforeGrow = delay,
                targetScale = stepObj.transform.localScale
            });
        }
    }
}
