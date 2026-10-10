using UnityEngine;

public class LevelAudioManager : MonoBehaviour
{
    [Header("Música de Fondo")]
    [SerializeField] private AudioSource bgmAudioSource;

    private void Start()
    {
        // Cargar el volumen maestro configurado por el jugador en las Opciones del Menú
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
        
        // Aplicar el volumen general de Unity y de la música del nivel
        AudioListener.volume = savedVolume;

        if (bgmAudioSource != null)
        {
            bgmAudioSource.volume = savedVolume;
            bgmAudioSource.loop = true;
            
            if (!bgmAudioSource.isPlaying)
            {
                bgmAudioSource.Play();
            }
        }
    }

    /// <summary>
    /// Método utilitario para reproducir efectos de sonido (SFX) sin cortar la música
    /// </summary>
    public static void PlaySFX(AudioSource source, AudioClip clip)
    {
        if (source != null && clip != null)
        {
            source.PlayOneShot(clip);
        }
    }
}