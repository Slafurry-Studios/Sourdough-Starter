using System.Collections;
using Slafurry.Core.Abstract;
using UnityEngine;

/// <summary>
/// Contoh System - layanan generik lintas-scene. Portable antar project.
/// </summary>
public class AudioSystem : GameSystem<AudioSystem>
{
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    protected override void OnSingletonAwake()
    {
        base.OnSingletonAwake(); // DontDestroyOnLoad
    }

    public override IEnumerator Initialize()
    {
        // contoh: load audio mixer / preload clip penting
        yield return null;
    }

    public override void PostInitialize()
    {
        // subscribe event global kalau perlu, misal GameManager.Instance.OnGameOver += ...
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }
}
