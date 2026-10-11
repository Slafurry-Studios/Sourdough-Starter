using Slafurry.Utils.Attributes;
using UnityEngine;


[System.Serializable]
public class SFXEffect
{
    public string groupID;
    public AudioClip[] clips;
    [Range(0f, 10f)]
    public float volume = 1f;
    public int maxSimultaneous = 3;

    [Header("Fade")]
    [Tooltip("Default fade-in duration (seconds) when this effect plays. 0 = instant full volume.")]
    [Range(0f, 5f)]
    public float fadeIn = 0f;
    [Tooltip("Default fade-out duration (seconds) when this effect stops. 0 = instant stop.")]
    [Range(0f, 5f)]
    public float fadeOut = 0f;

    [Header("Distance (used in PlaySFX3D)")]
    [Tooltip("When checked, sound only changes VOLUME with distance, without left/right panning. Suited for 2D/top-down games.")]
    public bool distanceOnlyNoPanning = true;
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;
    [Tooltip("Below this distance the volume is full 100%.")]
    public float minDistance = 1f;
    [Tooltip("Beyond this distance the sound is no longer audible at all.")]
    public float maxDistance = 15f;
}

namespace Slafurry.Systems.Audio
{

    [GameAssetCreator("Audio/SFX", "SFX Category", order: 2)]
    public class SFXCategory : ScriptableObject
    {
        public string categoryName;
        public SFXEffect[] effects;
        public int poolSize = 8;
    }
}