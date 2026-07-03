using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace HonamiAnimationSystem.Runtime.Core
{
    /// <summary>
    /// Base asset type for graph nodes that create and update playable state output.
    /// </summary>
    public abstract class HonamiNodeBase : ScriptableObject
    {
        [Tooltip("If true, non-deterministic node behaviors will be synchronized across linked animators during the exact same frame.")]
        public bool syncWhenLinked;

        public virtual bool IsVirtual => false;
        public virtual bool IsGlobal => false;

        public abstract Playable CreatePlayable(PlayableGraph graph, HonamiState state);

        public virtual Playable CreatePlayable(PlayableGraph graph, HonamiState state, Func<PlayableGraph, Playable> mirrorFactory)
            => CreatePlayable(graph, state);

        public abstract float GetDuration(HonamiState state, int stateIndex, Dictionary<int, int> pickedIdx, float blendParam);

        public virtual float GetUnscaledDuration(HonamiState state, int stateIndex, Dictionary<int, int> pickedIdx, float blendParam)
            => GetDuration(state, stateIndex, pickedIdx, blendParam);

        public virtual double GetCurrentTime(HonamiState state, int stateIndex, Playable playable)
            => playable.GetTime();

        public abstract void UpdateRuntime(in HonamiExecutionContext ctx);

        public virtual void OnEnter(in HonamiExecutionContext ctx)
        {
        }

        public virtual void OnExit(in HonamiExecutionContext ctx)
        {
        }

        public virtual void OnBuildMetadata(
            int stateIndex,
            HonamiState state,
            List<int> anyStateIndices,
            Dictionary<int, double> repeaterLastFireTime,
            Dictionary<int, int> repeaterFireCount)
        {
        }
    }
}
