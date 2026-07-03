using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace HonamiAnimationSystem.Runtime.Core
{
    /// <summary>
    /// Base asset type for runtime modifiers attached to a Honami state.
    /// </summary>
    public abstract class HonamiSubNodeBase : ScriptableObject
    {
        public virtual string DisplayName
        {
            get
            {
                string name = GetType().Name.Replace("Honami", "").Replace("SubNode", "");
                return Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
            }
        }

        public virtual bool RequiresMirroring => false;

        public abstract void UpdateRuntime(in HonamiExecutionContext ctx);

        public virtual void OnEnter(in HonamiExecutionContext ctx)
        {
        }

        public virtual void OnExit(in HonamiExecutionContext ctx)
        {
        }

        public virtual void GatherMasks(HashSet<HonamiAvatarMask> masks)
        {
        }
    }
}
