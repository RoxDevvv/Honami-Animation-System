using System;
using UnityEngine;

namespace HonamiAnimationSystem.Runtime.Timeline
{
    /// <summary>
    /// Serializable animation clip segment used by a Honami timeline animation track.
    /// </summary>
    [Serializable]
    public sealed class HonamiTimelineClip
    {
        public AnimationClip clip;
        public float startTime = 0f;
        public float durationOverride = 0f;
        public float speed = 1f;
        public bool reversed = false;
        public float clipStartOffset = 0f;

        /// <summary>
        /// Gets the playable duration of this timeline clip segment in seconds.
        /// </summary>
        public float GetDuration()
        {
            if (durationOverride > 0f)
            {
                return durationOverride;
            }

            if (clip == null)
            {
                return 0f;
            }

            float effectiveSpeed = speed != 0f ? Mathf.Abs(speed) : 1f;
            return (clip.length - clipStartOffset) / effectiveSpeed;
        }

        /// <summary>
        /// Converts timeline-local time into the sample time used by the underlying animation clip.
        /// </summary>
        public float GetSampleTime(float localTime)
        {
            float effectiveSpeed = speed != 0f ? Mathf.Abs(speed) : 1f;
            float sampleTime = clipStartOffset + localTime * effectiveSpeed;

            return reversed && clip != null
                ? clip.length - sampleTime
                : sampleTime;
        }
    }
}
