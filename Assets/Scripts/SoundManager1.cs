using UnityEngine;

public class SoundManager1 : MonoBehaviour
{
    [SerializeField] private AudioSource BackgroundMusic;
    [SerializeField] private AudioSource EffectsMusic;
    private bool isMuted = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void HandleMuteAudio()
    {
        if (isMuted)
        {
            isMuted = false;
            BackgroundMusic.mute = false;
            EffectsMusic.mute = false;
        }
        else
        {
            isMuted = true;
            BackgroundMusic.mute = true;
            EffectsMusic.mute = true;
        }
    }

}
