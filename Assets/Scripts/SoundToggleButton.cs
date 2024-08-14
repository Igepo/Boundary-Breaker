using UnityEngine;
using UnityEngine.UI;

public class SoundToggleButton : MonoBehaviour
{
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;

    private Image imageRenderer;
    private bool isSoundOn;

    void Start()
    {
        imageRenderer = GetComponent<Image>();
        isSoundOn = !MusicManager.instance.IsMuted() && !SoundManager.instance.IsMuted();
        UpdateButtonSprite();
    }

    public void OnClickedDown()
    {
        ToggleSound();
    }

    private void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        UpdateButtonSprite();

        if (isSoundOn)
        {
            MusicManager.instance.UnmuteMusic();
            SoundManager.instance.UnmuteSound();
        }
        else
        {
            MusicManager.instance.MuteMusic();
            SoundManager.instance.MuteSound();
        }
    }

    private void UpdateButtonSprite()
    {
        imageRenderer.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
    }
}
