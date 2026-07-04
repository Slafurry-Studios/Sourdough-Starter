using UnityEngine;

namespace Slafurry.System.Audio
{
    [CreateAssetMenu(fileName = "MusicData", menuName = "Game/Audio/Music Data")]
    public class MusicData : ScriptableObject
    {
        public MusicTrack[] tracks;

        public MusicTrack GetTrack(string trackName)
        {
            foreach (var track in tracks)
            {
                if (track.trackName == trackName)
                    return track;
            }
            return default;
        }

        public AudioClip GetClipFromName(string trackName)
        {
            return GetTrack(trackName).clip;
        }
    }
}