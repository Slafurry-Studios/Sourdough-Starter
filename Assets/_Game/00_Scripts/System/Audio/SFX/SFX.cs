using UnityEngine;


[System.Serializable]
public class SFXEffect
{
    public string groupID;
    public AudioClip[] clips;
    [Range(0f, 1f)]
    public float volume = 1f;
    public int maxSimultaneous = 3;
}

[System.Serializable]
public class SFXCategory
{
    public string categoryName;
    public SFXEffect[] effects;
    public int poolSize = 8;
}