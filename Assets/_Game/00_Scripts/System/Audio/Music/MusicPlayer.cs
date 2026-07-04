using UnityEngine;
using System.Collections;
using Slafurry.System.Scene;

namespace Slafurry.System.Audio
{
    public class MusicPlayer : MonoBehaviour
    {
        [SerializeField] private MusicData musicData;
        [SerializeField] private AudioSource musicSource;

        [Header("Scene Music")]
        [SerializeField] private SceneTrack[] sceneTracks;

        private Coroutine _currentFadeCoroutine;

        void OnEnable()
        {
            SceneLoader.Instance.OnSceneLoadCompleted += HandleSceneLoaded;
        }

        void OnDisable()
        {
            if (SceneLoader.Instance != null)
                SceneLoader.Instance.OnSceneLoadCompleted -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(string sceneName)
        {
            string trackToPlay = null;

            if (sceneTracks != null)
            {
                foreach (var st in sceneTracks)
                {
                    if (!string.IsNullOrEmpty(st.sceneName) && st.sceneName == sceneName)
                    {
                        trackToPlay = st.trackName;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(trackToPlay))
                trackToPlay = sceneName;

            if (musicData != null && musicData.GetClipFromName(trackToPlay) != null)
                PlayMusic(trackToPlay);
        }

        public void PlayMusic(string trackName, float fadeDuration = 0.5f)
        {
            if (musicData == null || musicSource == null) return;

            MusicTrack track = musicData.GetTrack(trackName);
            if (track.clip == null) return;

            if (_currentFadeCoroutine != null)
                StopCoroutine(_currentFadeCoroutine);

            _currentFadeCoroutine = StartCoroutine(AnimateMusicCrossfade(track.clip, track.volume, fadeDuration));
        }

        private IEnumerator AnimateMusicCrossfade(AudioClip nextTrack, float targetVolume, float fadeDuration = 0.5f)
        {
            float startVolume = musicSource.volume;
            float percent = 0;
            while (percent < 1)
            {
                percent += Time.unscaledDeltaTime / fadeDuration;
                musicSource.volume = Mathf.Lerp(startVolume, 0, percent);
                yield return null;
            }

            musicSource.clip = nextTrack;
            musicSource.Play();

            percent = 0;
            while (percent < 1)
            {
                percent += Time.unscaledDeltaTime / fadeDuration;
                musicSource.volume = Mathf.Lerp(0, targetVolume, percent);
                yield return null;
            }

            musicSource.volume = targetVolume;
            _currentFadeCoroutine = null;
        }
    }
}