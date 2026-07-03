using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace HonamiAnimationSystem.Runtime.Core
{
    /// <summary>
    /// Virtual global node that allows a state to re-fire repeatedly with cooldown limits.
    /// </summary>
    public sealed class HonamiRepeaterNode : HonamiNodeBase
    {
        [Tooltip("Minimum time (in seconds) between repeats.")]
        public float repeatCooldown = 0.1f;

        [Tooltip("Max repeats. 0 = infinite.")]
        public int maxRepeats = 0;

        public override bool IsVirtual => true;
        public override bool IsGlobal => true;

        public override Playable CreatePlayable(PlayableGraph graph, HonamiState state) => Playable.Null;

        public override float GetDuration(HonamiState state, int stateIndex, Dictionary<int, int> pickedIdx, float blendParam) => 0f;

        public override void UpdateRuntime(in HonamiExecutionContext ctx)
        {
        }

        public override void OnBuildMetadata(
            int stateIndex,
            HonamiState state,
            List<int> anyStateIndices,
            Dictionary<int, double> repeaterLastFireTime,
            Dictionary<int, int> repeaterFireCount)
        {
            anyStateIndices.Add(stateIndex);

            if (!repeaterLastFireTime.ContainsKey(stateIndex))
            {
                repeaterLastFireTime[stateIndex] = -999.0;
            }

            if (!repeaterFireCount.ContainsKey(stateIndex))
            {
                repeaterFireCount[stateIndex] = 0;
            }
        }
    }
}
