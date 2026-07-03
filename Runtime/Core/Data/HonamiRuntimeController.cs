using System.Collections.Generic;
using UnityEngine;

namespace HonamiAnimationSystem.Runtime.Core
{
    /// <summary>
    /// Base runtime-facing controller contract shared by regular and override Honami controllers.
    /// </summary>
    public abstract class HonamiRuntimeController : ScriptableObject
    {
        public abstract IReadOnlyList<HonamiLayer> ActiveLayers { get; }
        public abstract IReadOnlyList<HonamiParameter> ActiveParameters { get; }
        public abstract IReadOnlyList<HonamiState> ActiveStates { get; }

        /// <summary>
        /// Gets the node that should be evaluated for the given runtime state.
        /// </summary>
        public abstract HonamiNodeBase GetActiveNode(HonamiState state);

        /// <summary>
        /// Gets the active node for a state GUID after controller overrides are applied.
        /// </summary>
        public abstract HonamiNodeBase GetActiveNodeByGuid(string guid);

        /// <summary>
        /// Gets the transitions that should be evaluated for the given runtime state.
        /// </summary>
        public virtual IReadOnlyList<HonamiTransition> GetTransitions(HonamiState state)
        {
            if (state == null)
            {
                return null;
            }

            return state.transitions;
        }

#if UNITY_EDITOR
        public abstract HonamiController BaseController { get; }
        public abstract bool IsOverride { get; }
#endif
    }
}
