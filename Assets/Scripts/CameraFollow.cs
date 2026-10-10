using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // El jugador (Mack)
    public float smoothSpeed = 0.125f; // Suavizado del movimiento
    public Vector3 offset = new Vector3(0, 1, -10); // Distancia de la cámara

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}