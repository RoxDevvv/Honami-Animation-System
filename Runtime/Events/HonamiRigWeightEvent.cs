using UnityEngine;
using HonamiAnimationSystem.Runtime.Riggings;

namespace HonamiAnimationSystem.Runtime.Events
{
    [AddComponentMenu("Honami Animation/Events/Honami Rig Weight Event")]
    public sealed class HonamiRigWeightEvent : HonamiGlobalEvent
    {
        [Header("Target")]
        [Tooltip("The rig component to modify when this event is executed.")]
        public HonamiRig targetRig;

        [Header("Settings")]
        [Tooltip("The weight to apply to the rig (0 for off, 1 for fully active).")]
        [Range(0f, 1f)]
        public float targetWeight = 1f;

        public override void ExecuteEvent()
        {
            if (targetRig != null)
            {
                targetRig.weight = targetWeight;
            }
        }
    }
}
