using UnityEngine;

public class SoundManager : MonoBehaviour
{
     public static SoundManager Instance;
    private AudioSource audioSource;
    public AudioClip destroySound; // Agrega una variable para el sonido de destrucción
    public AudioClip zapSound;
    public AudioClip explosionSound; // Sonido para bombas
    public AudioClip lightningSound; // Sonido para rayos

    private void Awake()
    {
        // Configurar Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
    }
    private void Start()
    {
        // Obtén el componente AudioSource
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound()
    {
        // Reproduce el sonido de selección
        audioSource.Play();
    }

    public void PlayDestroySound()
    {
        // Reproduce el sonido de destrucción
        Debug.Log("Reproduciendo sonido de destrucción normal");
        audioSource.PlayOneShot(destroySound); // Usa PlayOneShot para reproducir un sonido sin interrumpir el actual
    }
        public void PlayNoMatchSound()
    {
        // Reproduce el sonido de zapSound
        audioSource.PlayOneShot(zapSound); // Usa PlayOneShot para reproducir un sonido sin interrumpir el actual
    }
     public void PlayExplosionSound()
    {
        Debug.Log("Reproduciendo sonido de bomba");
        audioSource.PlayOneShot(explosionSound);
    }

    public void PlayLightningSound()
    {
        Debug.Log("Reproduciendo sonido de rayo");
        audioSource.PlayOneShot(lightningSound);
    }
}
