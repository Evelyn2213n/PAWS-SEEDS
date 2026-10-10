using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Cámara a seguir. Si se deja vacío, tomará automáticamente la Main Camera.")]
    [SerializeField] private Transform cameraTransform;

    [Header("Efecto de Acompañamiento / Paralaje")]
    [Tooltip("1 = se mueve pegado a la cámara (100%), 0.5 = velocidad media (profundidad 2D), 0 = quieto.")]
    [Range(0f, 1f)]
    [SerializeField] private float parallaxEffectX = 0.5f;

    [Tooltip("Controla si el fondo también acompaña en el eje vertical Y.")]
    [Range(0f, 1f)]
    [SerializeField] private float parallaxEffectY = 0f;

    private Vector3 startPosition;
    private Vector3 startCameraPosition;

    private void Start()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        startPosition = transform.position;

        if (cameraTransform != null)
        {
            startCameraPosition = cameraTransform.position;
        }
    }

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        Vector3 cameraDelta = cameraTransform.position - startCameraPosition;

        transform.position = new Vector3(
            startPosition.x + (cameraDelta.x * parallaxEffectX),
            startPosition.y + (cameraDelta.y * parallaxEffectY),
            startPosition.z
        );
    }
}
