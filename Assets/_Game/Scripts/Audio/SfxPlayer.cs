using System.Collections.Generic;
using UnityEngine;

public class SfxPlayer : MonoBehaviour
{
    public static SfxPlayer Instance { get; private set; }

    public static event System.Action OnReady;

    [Header("Pool Sizes")]
    [SerializeField] private int _uiPoolSize = 5;
    [SerializeField] private int _playerPoolSize = 5;
    [SerializeField] private int _enemyPoolSize = 8;
    [SerializeField] private int _environmentPoolSize = 5;

    private AudioSource[] _uiPool;
    private AudioSource[] _playerPool;
    private AudioSource[] _enemyPool;
    private AudioSource[] _environmentPool;

    private const string SFX_VOLUME_KEY = "SfxVolume";
    private float _sfxVolume = 1f;

    private readonly Dictionary<AudioClip, AudioSource> _loopingSources = new();

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

        _uiPool = CreatePool("UI", _uiPoolSize);
        _playerPool = CreatePool("Player", _playerPoolSize);
        _enemyPool = CreatePool("Enemy", _enemyPoolSize);
        _environmentPool = CreatePool("Environment", _environmentPoolSize);
    }

    void Start()
    {
        _sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        OnReady?.Invoke();
    }

    // ─── Public Playback ──────────────────────────────────────────────────────

    public void PlayUISfx(AudioClip clip, float volume = 1f, bool loop = false) => PlayFromPool(_uiPool, clip, volume, loop);
    public void PlayPlayerSfx(AudioClip clip, float volume = 1f, bool loop = false) => PlayFromPool(_playerPool, clip, volume, loop);
    public void PlayEnemySfx(AudioClip clip, float volume = 1f, bool loop = false) => PlayFromPool(_enemyPool, clip, volume, loop);
    public void PlayEnvironmentSfx(AudioClip clip, float volume = 1f, bool loop = false) => PlayFromPool(_environmentPool, clip, volume, loop);

    // ─── Loop Control ─────────────────────────────────────────────────────────

    public void StopLoopingSfx(AudioClip clip)
    {
        if (clip == null) return;
        if (_loopingSources.TryGetValue(clip, out AudioSource source))
        {
            source.loop = false;
            source.Stop();
            _loopingSources.Remove(clip);
        }
    }

    public void StopAllLoopingSfx()
    {
        foreach (var kvp in _loopingSources)
        {
            kvp.Value.loop = false;
            kvp.Value.Stop();
        }
        _loopingSources.Clear();
    }

    public void SetLoopingVolume(AudioClip clip, float volume)
    {
        if (clip == null) return;
        if (_loopingSources.TryGetValue(clip, out AudioSource source))
            source.volume = Mathf.Clamp01(volume * _sfxVolume);
    }

    // ─── Volume ───────────────────────────────────────────────────────────────

    public void SetSfxVolume(float value)
    {
        _sfxVolume = value;
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, value);
        PlayerPrefs.Save();
    }

    public float GetSfxVolume() => _sfxVolume;

    // ─── Internal ─────────────────────────────────────────────────────────────

    private void PlayFromPool(AudioSource[] pool, AudioClip clip, float volume, bool loop)
    {
        if (clip == null) return;

        float finalVolume = Mathf.Clamp01(volume * _sfxVolume);

        if (loop)
        {
            if (_loopingSources.ContainsKey(clip)) return;
            AudioSource source = GetAvailableSource(pool);
            source.clip = clip;
            source.volume = finalVolume;
            source.loop = true;
            source.Play();
            _loopingSources[clip] = source;
        }
        else
        {
            AudioSource source = GetAvailableSource(pool);
            source.loop = false;
            source.PlayOneShot(clip, finalVolume);
        }
    }

    private AudioSource GetAvailableSource(AudioSource[] pool)
    {
        foreach (AudioSource source in pool)
            if (!source.isPlaying) return source;
        return pool[0];
    }

    private AudioSource[] CreatePool(string groupName, int size)
    {
        GameObject parent = new GameObject($"Pool_{groupName}");
        parent.transform.SetParent(transform);

        AudioSource[] pool = new AudioSource[size];
        for (int i = 0; i < size; i++)
        {
            GameObject go = new GameObject($"Source_{i}");
            go.transform.SetParent(parent.transform);
            pool[i] = go.AddComponent<AudioSource>();
            pool[i].playOnAwake = false;
        }
        return pool;
    }
}