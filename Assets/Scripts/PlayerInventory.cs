using System;
using UnityEngine;

/// <summary>
/// Gestiona el inventario de semillas recolectadas por Mack.
/// Notifica automáticamente a la UI cuando las semillas cambian.
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    [Header("--- Configuración de Semillas ---")]
    [Tooltip("Cantidad actual de semillas.")]
    public int seedCount = 5;

    [Tooltip("Capacidad máxima de semillas que puede acumular el jugador.")]
    public int maxSeeds = 10;

    [Header("--- Semilla Milagrosa ---")]
    [Tooltip("Indica si el jugador ha obtenido la semilla milagrosa.")]
    public bool hasMiracleSeed = false;

    /// <summary>
    /// Evento disparado cuando cambia la cantidad de semillas (current, max).
    /// </summary>
    public event Action<int, int> OnSeedsChanged;

    private void Start()
    {
        // Notificar el estado inicial
        NotifySeedsChanged();
    }

    /// <summary>
    /// Intenta consumir una semilla para plantar.
    /// </summary>
    /// <returns>True si se pudo descontar una semilla, False si no habían suficientes.</returns>
    public bool UseSeed()
    {
        if (seedCount > 0)
        {
            seedCount--;
            Debug.Log($"[PlayerInventory] Semilla sembrada. Restantes: {seedCount}/{maxSeeds}");
            NotifySeedsChanged();
            return true;
        }

        Debug.Log("[PlayerInventory] ¡No tienes semillas!");
        return false;
    }

    /// <summary>
    /// Añade semillas al inventario respetando el límite máximo.
    /// </summary>
    /// <param name="amount">Cantidad a añadir.</param>
    /// <returns>True si se pudo añadir al menos una semilla.</returns>
    public bool AddSeed(int amount)
    {
        if (amount <= 0) return false;

        if (seedCount >= maxSeeds)
        {
            Debug.Log("[PlayerInventory] Bolsa de semillas llena.");
            return false;
        }

        seedCount = Mathf.Clamp(seedCount + amount, 0, maxSeeds);
        Debug.Log($"[PlayerInventory] Semilla recolectada (+{amount}). Total: {seedCount}/{maxSeeds}");
        NotifySeedsChanged();
        return true;
    }

    /// <summary>
    /// Establece la cantidad de semillas manualmente y actualiza la UI.
    /// </summary>
    public void SetSeeds(int amount, int newMax = -1)
    {
        if (newMax > 0) maxSeeds = newMax;
        seedCount = Mathf.Clamp(amount, 0, maxSeeds);
        NotifySeedsChanged();
    }

    private void NotifySeedsChanged()
    {
        OnSeedsChanged?.Invoke(seedCount, maxSeeds);
    }
}