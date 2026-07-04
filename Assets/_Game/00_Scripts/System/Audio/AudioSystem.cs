using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Slafurry.Core.Abstract;

namespace Slafurry.System.Audio
{
    public static class Audio
    {
        public static void PlayMusic(string trackName, float fade = 0.5f) => AudioSystem.Music.PlayMusic(trackName, fade);
        public static void PlaySFX2D(string category, string effect) => AudioSystem.SFX.PlaySFX2D(category, effect);
        public static void PlaySFX3D(string category, string effect, Vector3 pos) => AudioSystem.SFX.PlaySFX3D(category, effect, pos);
    }
    
    public class AudioSystem : GameSystem<AudioSystem>
    {
        [Header("Mixer")]
        [SerializeField] private AudioMixer audioMixer;

        [Header("Sub Players")]
        [SerializeField] private MusicPlayer musicPlayer;
        [SerializeField] private SFXPlayer sfxPlayer;

        public static MusicPlayer Music => Instance.musicPlayer;
        public static SFXPlayer SFX => Instance.sfxPlayer;

        public event Action<float> OnMusicVolumeChanged;
        public event Action<float> OnSFXVolumeChanged;

        private const string MusicKey = "MusicVolume";
        private const string SFXKey = "SFXVolume";

        // ======================== GAME SYSTEM LIFECYCLE ========================
        public override IEnumerator Initialize()
        {
            LoadVolume();
            yield return new WaitForSecondsRealtime(0.1f);
        }

        public override void PostInitialize()
        {
            UpdateMusicVolume(PlayerPrefs.GetFloat(MusicKey, 1f));
            UpdateSFXVolume(PlayerPrefs.GetFloat(SFXKey, 1f));
            PlaySceneMusic();
        }

        // ======================== VOLUME LOADER ========================
        public void LoadVolume()
        {
            // dibaca disini, di-apply nyata pas PostInitialize() lewat UpdateMusicVolume/UpdateSFXVolume
            // biar gak dobel nulis ke PlayerPrefs
        }

        // ======================== PUBLIC API ========================
        public void PlaySceneMusic()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            musicPlayer.PlayMusic(currentSceneName);
        }

        // ======================== VOLUME CHANGER ========================
        public void UpdateMusicVolume(float linearVolume)
        {
            audioMixer.SetFloat(MusicKey, LinearToDecibel(linearVolume));
            PlayerPrefs.SetFloat(MusicKey, linearVolume);
            OnMusicVolumeChanged?.Invoke(linearVolume);
        }

        public void UpdateSFXVolume(float linearVolume)
        {
            audioMixer.SetFloat(SFXKey, LinearToDecibel(linearVolume));
            PlayerPrefs.SetFloat(SFXKey, linearVolume);
            OnSFXVolumeChanged?.Invoke(linearVolume);
        }

        private float LinearToDecibel(float linear)
            => linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();
        }
    }
}

