using UnityEngine;

public class ChangeButtonSprites : MonoBehaviour
{
    public Sprite UnmutedSprite;
    public Sprite MutedSprite;
    public UnityEngine.UI.Button button;
    [SerializeField] private AudioSource BackgroundMusic;
    [SerializeField] private AudioSource EffectsMusic;
    public bool isMuted = false;
    public void ChangeSprite()
    {
        if (isMuted)
        {
            isMuted = false;
            button.image.sprite = UnmutedSprite;
            BackgroundMusic.mute = false;
            EffectsMusic.mute = false;
        }
        else
        {
            isMuted = true;
            button.image.sprite = MutedSprite;
            BackgroundMusic.mute = true;
            EffectsMusic.mute = true;
        }
    }
}
