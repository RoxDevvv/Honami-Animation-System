using UnityEngine;

namespace HonamiAnimationSystem.Runtime.Core
{
    /// <summary>
    /// Scriptable identifier used to broadcast linked animation actions without string coupling.
    /// </summary>
    [CreateAssetMenu(fileName = "New Action ID", menuName = "Honami Animation/Action ID")]
    public sealed class HonamiActionID : ScriptableObject
    {
        [Tooltip("Optional descriptive name or identifier for debugging.")]
        public string actionName;
    }
}
