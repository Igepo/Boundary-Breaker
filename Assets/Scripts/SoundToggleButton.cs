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
        isSoundOn = !MusicManager.Instance.IsMuted() && !SoundManager.Instance.IsMuted();
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
            MusicManager.Instance.UnmuteMusic();
            SoundManager.Instance.UnmuteSound();
        }
        else
        {
            MusicManager.Instance.MuteMusic();
            SoundManager.Instance.MuteSound();
        }
    }

    private void UpdateButtonSprite()
    {
        imageRenderer.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
    }
}
