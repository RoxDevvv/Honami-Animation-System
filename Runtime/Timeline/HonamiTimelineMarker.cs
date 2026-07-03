using System;

namespace HonamiAnimationSystem.Runtime.Timeline
{
    /// <summary>
    /// Base timeline marker data shared by specialized marker types.
    /// </summary>
    [Serializable]
    public abstract class HonamiTimelineMarker
    {
        public string id = "Marker";
        public float time = 0f;
    }

    /// <summary>
    /// Marker describing a loopable section between <see cref="HonamiTimelineMarker.time"/> and <see cref="endTime"/>.
    /// </summary>
    [Serializable]
    public sealed class HonamiTimelineLoopMarker : HonamiTimelineMarker
    {
        public float endTime = 5f;

        public HonamiTimelineLoopMarker()
        {
            id = "Loop";
        }
    }
}
