using System;
using System.Collections.Generic;
using UnityEngine;

namespace PawsAndSeeds.Platforms
{
    /// <summary>
    /// Gestor centralizado y generador de plataformas para Paws & Seeds (Unity 6).
    /// Permite instanciar y configurar plataformas estáticas, flotantes y móviles tanto en tiempo de ejecución
    /// como mediante configuración de datos en el Inspector.
    /// </summary>
    public class PlatformManager : MonoBehaviour
    {
        public static PlatformManager Instance { get; private set; }

        public enum PlatformCategory
        {
            StaticFloating,
            MovingHorizontal,
            MovingVertical,
            OneWayThrough,
            GrowableVineBridge
        }

        [Serializable]
        public class PlatformSpawnData
        {
            [Tooltip("Identificador o etiqueta de la plataforma.")]
            public string platformName = "Plataforma";

            [Tooltip("Tipo de plataforma a instanciar.")]
            public PlatformCategory category = PlatformCategory.StaticFloating;

            [Tooltip("Posición en el mundo donde se colocará.")]
            public Vector2 spawnPosition;

            [Tooltip("Escala del sprite/collider (X: ancho, Y: grosor).")]
            public Vector2 scale = Vector2.one;

            [Header("Ajustes si es Móvil")]
            [Tooltip("Distancia que recorrerá la plataforma.")]
            public float travelDistance = 4f;

            [Tooltip("Velocidad de traslación.")]
            public float speed = 2.5f;

            [Tooltip("¿Requiere que Mack la pise para activarse?")]
            public bool activateOnTouch = false;
        }

        [Header("--- Prefabs Base (Opcionales) ---")]
        [Tooltip("Prefab base para plataforma estática. Si está vacío, se creará dinámicamente usando el sprite del suelo.")]
        [SerializeField] private GameObject staticPlatformPrefab;

        [Tooltip("Prefab base para plataforma móvil.")]
        [SerializeField] private GameObject movingPlatformPrefab;

        [Tooltip("Prefab base para puentes/escaleras de enredaderas que crecen.")]
        [SerializeField] private GameObject growablePlantPrefab;

        [Header("--- Recursos por Defecto ---")]
        [Tooltip("Sprite de la plataforma (por defecto FONDO PAWS_SUELO).")]
        [SerializeField] private Sprite groundPlatformSprite;

        [Tooltip("Material de física 2D sin fricción para bordes.")]
        [SerializeField] private PhysicsMaterial2D platformPhysicsMaterial;

        [Tooltip("Capa que se asignará a las plataformas creadas (por defecto 'Ground').")]
        [SerializeField] private string groundLayerName = "Ground";

        [Header("--- Lista de Plataformas del Nivel ---")]
        [Tooltip("Lista de plataformas para instanciar automáticamente al inicio del nivel.")]
        [SerializeField] private List<PlatformSpawnData> levelPlatforms = new List<PlatformSpawnData>();

        [Header("--- Contenedor de la Jerarquía ---")]
        [Tooltip("Transform padre donde se organizarán las plataformas instanciadas.")]
        [SerializeField] private Transform platformsContainer;

        // Registro de plataformas activas en la escena
        private readonly List<GameObject> activePlatforms = new List<GameObject>();

        private void Awake()
        {
            if (Instance != null && Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (platformsContainer == null)
            {
                platformsContainer = transform;
            }
        }

        private void Start()
        {
            SpawnPredefinedPlatforms();
        }

        /// <summary>
        /// Genera todas las plataformas configuradas en la lista del Inspector.
        /// </summary>
        public void SpawnPredefinedPlatforms()
        {
            foreach (var data in levelPlatforms)
            {
                SpawnPlatform(data);
            }
        }

        /// <summary>
        /// Instancia una plataforma basada en su configuración de datos.
        /// </summary>
        public GameObject SpawnPlatform(PlatformSpawnData data)
        {
            GameObject platformObj = null;

            switch (data.category)
            {
                case PlatformCategory.StaticFloating:
                    platformObj = CreateStaticPlatform(data.spawnPosition, data.scale, data.platformName);
                    break;

                case PlatformCategory.MovingHorizontal:
                    platformObj = CreateMovingPlatform(data.spawnPosition, MovingPlatform.MovementType.Horizontal, data.travelDistance, data.speed, data.activateOnTouch, data.scale, data.platformName);
                    break;

                case PlatformCategory.MovingVertical:
                    platformObj = CreateMovingPlatform(data.spawnPosition, MovingPlatform.MovementType.Vertical, data.travelDistance, data.speed, data.activateOnTouch, data.scale, data.platformName);
                    break;

                case PlatformCategory.OneWayThrough:
                    platformObj = CreateOneWayPlatform(data.spawnPosition, data.scale, data.platformName);
                    break;

                case PlatformCategory.GrowableVineBridge:
                    platformObj = CreateGrowablePlantBridge(data.spawnPosition, data.platformName);
                    break;
            }

            if (platformObj != null)
            {
                activePlatforms.Add(platformObj);
            }

            return platformObj;
        }

        /// <summary>
        /// Crea una plataforma flotante estática.
        /// </summary>
        public GameObject CreateStaticPlatform(Vector2 position, Vector2 scale, string name = "Static_Platform")
        {
            GameObject go;
            if (staticPlatformPrefab != null)
            {
                go = Instantiate(staticPlatformPrefab, position, Quaternion.identity, platformsContainer);
            }
            else
            {
                go = new GameObject(name);
                go.transform.position = position;
                go.transform.SetParent(platformsContainer);

                // Configurar SpriteRenderer
                var sr = go.AddComponent<SpriteRenderer>();
                if (groundPlatformSprite != null)
                {
                    sr.sprite = groundPlatformSprite;
                }

                // Configurar BoxCollider2D con bordes redondeados (anti-stuck)
                var box = go.AddComponent<BoxCollider2D>();
                box.edgeRadius = 0.04f;
                if (platformPhysicsMaterial != null)
                {
                    box.sharedMaterial = platformPhysicsMaterial;
                }
            }

            go.transform.localScale = new Vector3(scale.x, scale.y, 1f);
            ApplyGroundLayer(go);
            return go;
        }

        /// <summary>
        /// Crea una plataforma móvil horizontal o vertical.
        /// </summary>
        public GameObject CreateMovingPlatform(Vector2 position, MovingPlatform.MovementType movementType, float distance, float speed, bool activateOnTouch, Vector2 scale, string name = "Moving_Platform")
        {
            GameObject go;
            if (movingPlatformPrefab != null)
            {
                go = Instantiate(movingPlatformPrefab, position, Quaternion.identity, platformsContainer);
            }
            else
            {
                go = new GameObject(name);
                go.transform.position = position;
                go.transform.SetParent(platformsContainer);

                // SpriteRenderer
                var sr = go.AddComponent<SpriteRenderer>();
                if (groundPlatformSprite != null)
                {
                    sr.sprite = groundPlatformSprite;
                }

                // Rigidbody2D Kinemático
                var rb = go.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.interpolation = RigidbodyInterpolation2D.Interpolate;

                // BoxCollider2D con EdgeRadius
                var box = go.AddComponent<BoxCollider2D>();
                box.edgeRadius = 0.04f;
                if (platformPhysicsMaterial != null)
                {
                    box.sharedMaterial = platformPhysicsMaterial;
                }

                // Componente MovingPlatform
                var moving = go.AddComponent<MovingPlatform>();
                // Ajustamos propiedades privadas usando reflexión o método público
                ConfigureMovingPlatform(moving, movementType, distance, speed, activateOnTouch);
            }

            go.transform.localScale = new Vector3(scale.x, scale.y, 1f);
            ApplyGroundLayer(go);
            return go;
        }

        /// <summary>
        /// Crea una plataforma unidireccional (Mack puede saltar desde abajo y apoyarse arriba).
        /// </summary>
        public GameObject CreateOneWayPlatform(Vector2 position, Vector2 scale, string name = "OneWay_Platform")
        {
            GameObject go = CreateStaticPlatform(position, scale, name);
            var box = go.GetComponent<BoxCollider2D>();
            if (box != null)
            {
                box.usedByEffector = true;
            }

            var effector = go.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.useOneWayGrouping = true;
            effector.surfaceArc = 170f;

            return go;
        }

        /// <summary>
        /// Crea una enredadera o puente de plantas escalable.
        /// </summary>
        public GameObject CreateGrowablePlantBridge(Vector2 position, string name = "Growable_Plant_Bridge")
        {
            GameObject go = null;
            if (growablePlantPrefab != null)
            {
                go = Instantiate(growablePlantPrefab, position, Quaternion.identity, platformsContainer);
                var growable = go.GetComponent<GrowablePlantPlatform>();
                if (growable != null)
                {
                    growable.GrowPlant();
                }
            }
            return go;
        }

        private void ConfigureMovingPlatform(MovingPlatform platform, MovingPlatform.MovementType mode, float distance, float speed, bool activateOnTouch)
        {
            var type = typeof(MovingPlatform);
            var modeField = type.GetField("movementType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var distField = type.GetField("travelDistance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var speedField = type.GetField("moveSpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var touchField = type.GetField("activateOnTouch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (modeField != null) modeField.SetValue(platform, mode);
            if (distField != null) distField.SetValue(platform, distance);
            if (speedField != null) speedField.SetValue(platform, speed);
            if (touchField != null) touchField.SetValue(platform, activateOnTouch);

            platform.SetupWaypoints();
        }

        private void ApplyGroundLayer(GameObject go)
        {
            int layerId = LayerMask.NameToLayer(groundLayerName);
            if (layerId != -1)
            {
                go.layer = layerId;
            }
        }

        /// <summary>
        /// Limpia y reinicia todas las plataformas dinámicas creadas.
        /// </summary>
        [ContextMenu("Eliminar Plataformas Instanciadas")]
        public void ClearActivePlatforms()
        {
            foreach (var p in activePlatforms)
            {
                if (p != null) DestroyImmediate(p);
            }
            activePlatforms.Clear();
        }

        /// <summary>
        /// Genera de inmediato las plataformas en el editor para previsualización.
        /// </summary>
        [ContextMenu("Generar Plataformas de Prueba")]
        public void GenerateTestPlatformsInEditor()
        {
            ClearActivePlatforms();
            SpawnPredefinedPlatforms();
        }
    }
}
