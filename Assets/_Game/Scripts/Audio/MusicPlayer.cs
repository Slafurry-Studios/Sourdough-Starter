using System.Collections;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _musicSource;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 1.5f;

    public static event System.Action OnReady;

    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private float _targetMusicVolume = 1f;

    private float _currentRelativeVolume = 1f;
    private Coroutine _fadeCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

    }

    void Start()
    {
        LoadVolume();
        OnReady?.Invoke();
    }

    // ─── Music ────────────────────────────────────────────────────────────────

    public void PlayMusic(AudioClip clip, float relativeVolume = 1f)
    {
        if (_musicSource.clip == clip) return;

        _musicSource.clip = clip;
        _musicSource.loop = true;
        _currentRelativeVolume = relativeVolume; // simpan untuk fade

        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(PlayWithFadeIn(fadeInDuration));
    }

    private IEnumerator PlayWithFadeIn(float duration)
    {
        float targetVolume = _targetMusicVolume * _currentRelativeVolume;

        _musicSource.volume = 0f;
        _musicSource.Play();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
            yield return null;
        }

        _musicSource.volume = targetVolume;
        _fadeCoroutine = null;
    }

    // ─── Volume ───────────────────────────────────────────────────────────────

    public void SetMusicVolume(float value)
    {
        _targetMusicVolume = value;

        if (_fadeCoroutine == null)
            _musicSource.volume = value * _currentRelativeVolume;

        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
        PlayerPrefs.Save();
    }

    public float GetMusicVolume() => _targetMusicVolume;

    private void LoadVolume()
    {
        _targetMusicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
    }
}