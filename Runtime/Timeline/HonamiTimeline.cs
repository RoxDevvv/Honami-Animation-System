using System.Collections.Generic;
using UnityEngine;

namespace HonamiAnimationSystem.Runtime.Timeline
{
    /// <summary>
    /// Scriptable timeline asset containing Honami animation and event tracks.
    /// </summary>
    [CreateAssetMenu(fileName = "New HonamiTimeline", menuName = "Honami Animation/Timeline", order = 10)]
    public sealed class HonamiTimeline : ScriptableObject
    {
        public string timelineName = "New Timeline";
        public float durationOverride = 0f;
        public List<HonamiTimelineTrack> tracks = new();

        [SerializeReference]
        public List<HonamiTimelineMarker> markers = new();

        /// <summary>
        /// Gets the timeline duration from the override value or from the latest track content.
        /// </summary>
        public float GetDuration()
        {
            if (durationOverride > 0f)
            {
                return durationOverride;
            }

            float max = 1f;

            foreach (var track in tracks)
            {
                float duration = track.GetDuration();
                if (duration > max)
                {
                    max = duration;
                }
            }

            return max;
        }
    }
}
