using System;
using UnityEngine.Events;

namespace HonamiAnimationSystem.Runtime.Timeline
{
    /// <summary>
    /// Defines how a timeline event is dispatched when its marker time is reached.
    /// </summary>
    public enum HonamiTimelineEventType
    {
        Broadcast,
        UnityEvent
    }

    /// <summary>
    /// Serializable event marker fired by a Honami timeline event track.
    /// </summary>
    [Serializable]
    public sealed class HonamiTimelineEvent
    {
        public float time = 0f;
        public string eventId = "NewEvent";
        public HonamiTimelineEventType eventType = HonamiTimelineEventType.Broadcast;
        public UnityEvent onFire;

        [NonSerialized]
        public bool hasFired;
    }
}
