using System.Collections.Generic;
using UnityEngine.Playables;

namespace HonamiAnimationSystem.Runtime.Core
{
    /// <summary>
    /// Virtual node used as an explicit graph exit target.
    /// </summary>
    public sealed class HonamiExitNode : HonamiNodeBase
    {
        public override bool IsVirtual => false;

        public override Playable CreatePlayable(PlayableGraph graph, HonamiState state) => Playable.Null;

        public override float GetDuration(HonamiState state, int stateIndex, Dictionary<int, int> pickedIdx, float blendParam) => 0f;

        public override void UpdateRuntime(in HonamiExecutionContext ctx)
        {
        }
    }
}
