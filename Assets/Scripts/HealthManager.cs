using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth;

    [Header("Configuración de UI")]
    public Image[] heartImages; // Arreglo de las imágenes UI
    public Sprite fullHeart;    // Sprite Vida1
    public Sprite emptyHeart;   // Sprite Sinvida1

    [Header("Pantalla de Derrota")]
    public GameObject gameOverPanel;
    public AudioSource audioSource;
    public AudioClip defeatSound;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHeartsUI();
        
        if (audioSource != null) 
        {
            audioSource.ignoreListenerPause = true;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHeartsUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHeartsUI();
    }

    void UpdateHeartsUI()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < currentHealth)
            {
                heartImages[i].sprite = fullHeart;
            }
            else
            {
                heartImages[i].sprite = emptyHeart;
            }
        }
    }

    private bool isDead = false;

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("¡Mack se ha quedado sin vidas!");
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (audioSource != null && defeatSound != null)
        {
            audioSource.PlayOneShot(defeatSound);
        }

        // Pausar el juego (opcional, pero útil para pantallas de derrota)
        Time.timeScale = 0f;
    }
}