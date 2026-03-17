using UnityEngine;

public class MusicStarter : MonoBehaviour
{
    [SerializeField] private AudioClip music;
    [Range(0f, 10f)]
    [SerializeField] private float relativeVolume = 1f;

    private void OnEnable()
    {
        if (MusicPlayer.Instance != null)
            MusicPlayer.Instance.PlayMusic(music, relativeVolume);
        else
            MusicPlayer.OnReady += Play;
    }

    private void Play()
    {
        MusicPlayer.Instance.PlayMusic(music, relativeVolume);
        MusicPlayer.OnReady -= Play;
    }

    private void OnDisable()
    {
        MusicPlayer.OnReady -= Play;
    }
}