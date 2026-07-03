using System;
using System.Collections.Generic;
using UnityEngine;

namespace HonamiAnimationSystem.Runtime.Timeline
{
    /// <summary>
    /// Defines the data a timeline track evaluates.
    /// </summary>
    public enum HonamiTimelineTrackType
    {
        Animation,
        Event
    }

    /// <summary>
    /// Serializable track containing animation clips or timeline events.
    /// </summary>
    [Serializable]
    public sealed class HonamiTimelineTrack
    {
        public string trackName = "Track";
        public HonamiTimelineTrackType trackType = HonamiTimelineTrackType.Animation;
        public GameObject target;
        public bool muted = false;
        public List<HonamiTimelineClip> clips = new();
        public List<HonamiTimelineEvent> events = new();

        /// <summary>
        /// Gets the furthest animation or event time on this track.
        /// </summary>
        public float GetDuration()
        {
            float max = 0f;

            foreach (var clip in clips)
            {
                float end = clip.startTime + clip.GetDuration();
                if (end > max)
                {
                    max = end;
                }
            }

            foreach (var evt in events)
            {
                if (evt.time > max)
                {
                    max = evt.time;
                }
            }

            return max;
        }
    }
}
