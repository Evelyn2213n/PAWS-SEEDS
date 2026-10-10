using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Script para gestionar el estado del jefe final.
/// Cuando es derrotado, permite avanzar con la semilla milagrosa.
/// </summary>
public class BossManager : MonoBehaviour
{
    [Header("Estado Global")]
    [Tooltip("Variable estática para saber si el jefe ha sido derrotado en cualquier parte del código.")]
    public static bool isBossDefeated = false;

    [Header("Eventos")]
    public UnityEvent onBossDefeated;

    private void Start()
    {
        // Reiniciamos la variable al inicio de la escena
        isBossDefeated = false;
    }

    /// <summary>
    /// Llama a esta función cuando la vida del jefe llegue a 0.
    /// </summary>
    public void DefeatBoss()
    {
        if (isBossDefeated) return;

        isBossDefeated = true;
        onBossDefeated?.Invoke();
        
        Debug.Log("[BossManager] Jefe derrotado. Ahora puedes plantar la semilla milagrosa en el agujero.");
        
        // Aquí puedes reproducir animaciones o partículas
        // ...

        // Destruir el objeto del jefe después de unos segundos
        Destroy(gameObject, 2f);
    }
}
