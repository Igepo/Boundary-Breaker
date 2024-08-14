using System;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance = null;

    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip[] _musicClips;
    private int _currentTrackIndex = 0;

    private bool _shouldPlayMusic = true;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (_musicClips.Length > 0)
        {
            _audioSource.clip = _musicClips[_currentTrackIndex];
            _audioSource.Play();
        }
    }

    void Update()
    {
        if (!_audioSource.isPlaying && _shouldPlayMusic)
        {
            PlayNextTrack();
        }
    }
    public void StopMusic()
    {
        _audioSource.Stop();
        _shouldPlayMusic = false;
    }
    public void ResumeMusic()
    {
        _shouldPlayMusic = true;
        if (!_audioSource.isPlaying)
        {
            _audioSource.Play();
        }
    }
    void PlayNextTrack()
    {
        _currentTrackIndex = (_currentTrackIndex + 1) % _musicClips.Length;
        _audioSource.clip = _musicClips[_currentTrackIndex];
        _audioSource.Play();
    }

    public void MuteMusic()
    {
        _audioSource.mute = true;
    }

    public void UnmuteMusic()
    {
        _audioSource.mute = false;
    }
    public bool IsMuted()
    {
        return _audioSource.mute;
    }
}