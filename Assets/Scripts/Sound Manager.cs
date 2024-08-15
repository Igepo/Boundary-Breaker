using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; } = null;

    [SerializeField] private AudioSource effectSource;
    [SerializeField] private Sound[] sounds;

    private Dictionary<string, AudioClip> soundDictionary;
    private bool isMuted = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        // Initialiser le dictionnaire
        soundDictionary = new Dictionary<string, AudioClip>();
        foreach (Sound sound in sounds)
        {
            soundDictionary[sound.name] = sound.clip;
        }
    }

    public void PlaySound(string soundName)
    {
        if (soundDictionary.ContainsKey(soundName))
        {
            effectSource.PlayOneShot(soundDictionary[soundName]);
        }
        else
        {
            Debug.LogWarning("Sound name not found: " + soundName);
        }
    }
    public void MuteSound()
    {
        effectSource.mute = true;
        isMuted = true;
    }

    public void UnmuteSound()
    {
        effectSource.mute = false;
        isMuted = false;
    }

    public bool IsMuted()
    {
        return isMuted;
    }
}

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
}
