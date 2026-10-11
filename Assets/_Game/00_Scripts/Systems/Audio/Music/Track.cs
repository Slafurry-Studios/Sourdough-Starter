using UnityEngine;

[System.Serializable]
public struct SceneTrack
{
    public string sceneName;
    public string trackName;
}

[System.Serializable]
public struct MusicTrack
{
    public string trackName;
    public AudioClip clip;
    [Tooltip("Intro clip played once before the main clip (loop). Leave empty if there is no intro.")]
    public AudioClip introClip;
    [Range(0f, 20f)]
    public float volume;
    [Tooltip("Fade in duration when this track starts playing.")]
    [SerializeField] private float fadeIn;
    [Tooltip("Fade out duration when this track stops or changes.")]
    [SerializeField] private float fadeOut;

    public float FadeIn => fadeIn > 0f ? fadeIn : 1f;
    public float FadeOut => fadeOut > 0f ? fadeOut : 1f;
}
