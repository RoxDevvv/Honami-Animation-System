using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace HonamiAnimationSystem.Runtime.Events
{
    public abstract class HonamiGlobalEvent : MonoBehaviour
    {
        [Tooltip("A unique identifier to distinguish this global event type or instance.")]
        public string eventId;

        /// <summary>
        /// Called when the timeline event marker hits this event's execution time.
        /// </summary>
        public abstract void ExecuteEvent();
    }
}
