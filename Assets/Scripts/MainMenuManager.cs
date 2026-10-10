using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [Tooltip("Nombre de la escena del nivel a cargar cuando se pulsa Jugar")]
    [SerializeField] private string gameSceneName = "Level 1";

    [Header("Pantalla de Inicio (Press E)")]
    [Tooltip("Elemento o botón de 'Pulsa E para comenzar'")]
    [SerializeField] private GameObject pressEPrompt;
    [Tooltip("Panel con los botones principales (Jugar, Historia, Opciones, etc.)")]
    [SerializeField] private GameObject menuButtonsPanel;

    [Header("Paneles Secundarios")]
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Ajustes de Audio")]
    [Tooltip("Componente AudioSource que reproducirá la música de fondo del menú")]
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumeText;

    private bool hasStarted = false;

    private void Start()
    {
        // Cargar volumen guardado (por defecto 0.8f = 80%)
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 0.8f);

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        // Aplicar el volumen inicial a la música y al Listener general
        SetMasterVolume(savedVolume);

        // Iniciar la música de fondo si está asignada y no se está reproduciendo
        if (bgmAudioSource != null)
        {
            bgmAudioSource.loop = true;
            if (!bgmAudioSource.isPlaying)
            {
                bgmAudioSource.Play();
            }
        }

        // Estado inicial de paneles
        if (pressEPrompt != null && menuButtonsPanel != null)
        {
            pressEPrompt.SetActive(true);
            menuButtonsPanel.SetActive(false);
        }
        else if (menuButtonsPanel != null)
        {
            menuButtonsPanel.SetActive(true);
        }

        CloseAllPopups();
    }

    private void Update()
    {
        // Si aún no se ha presionado E para abrir el menú
        if (!hasStarted && pressEPrompt != null && pressEPrompt.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                OnPressEStart();
            }
        }

        // Tecla ESC para volver o cerrar cualquier panel abierto
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseAllPopups();
        }
    }

    /// <summary>
    /// Transición desde el cartel 'PULSA [E]' a los botones del menú
    /// </summary>
    public void OnPressEStart()
    {
        hasStarted = true;
        if (pressEPrompt != null) pressEPrompt.SetActive(false);
        if (menuButtonsPanel != null) menuButtonsPanel.SetActive(true);
    }

    #region Navegación y Acciones Principales

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    #endregion

    #region Gestión de Paneles

    public void OpenStory()
    {
        CloseAllPopups();
        if (storyPanel != null) storyPanel.SetActive(true);
    }

    public void OpenControls()
    {
        CloseAllPopups();
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    public void OpenSettings()
    {
        CloseAllPopups();
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseAllPopups()
    {
        if (storyPanel != null) storyPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    #endregion

    #region Ajustes de Audio

    public void SetMasterVolume(float volume)
    {
        // Ajusta el volumen global de Unity y de la música
        AudioListener.volume = volume;
        
        if (bgmAudioSource != null)
        {
            bgmAudioSource.volume = volume;
        }

        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();

        if (volumeText != null)
        {
            volumeText.text = Mathf.RoundToInt(volume * 100f) + "%";
        }
    }

    #endregion
}