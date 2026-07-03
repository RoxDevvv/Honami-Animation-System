using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace HonamiAnimationSystem.Runtime.Core
{
    /// <summary>
    /// Honami state node that plays one weighted random animation clip.
    /// </summary>
    public sealed class HonamiRandomAnimationNode : HonamiNodeBase
    {
        public List<HonamiRandomAnimationClip> randomClips = new();

        public override Playable CreatePlayable(PlayableGraph graph, HonamiState state)
            => CreatePlayable(graph, state, null);

        public override Playable CreatePlayable(PlayableGraph graph, HonamiState state, Func<PlayableGraph, Playable> mirrorFactory)
        {
            if (randomClips == null)
            {
                return Playable.Null;
            }

            AnimationMixerPlayable mixer = AnimationMixerPlayable.Create(graph, randomClips.Count);

            for (int i = 0; i < randomClips.Count; i++)
            {
                HonamiRandomAnimationClip randomClip = randomClips[i];
                if (randomClip.clip == null)
                {
                    continue;
                }

                Playable clipOutput = CreateClipOutput(graph, state, randomClip, mirrorFactory);
                graph.Connect(clipOutput, 0, mixer, i);
                mixer.SetInputWeight(i, 0f);
            }

            return mixer;
        }

        public override float GetDuration(HonamiState state, int stateIndex, Dictionary<int, int> pickedIdx, float blendParam)
        {
            if (randomClips == null || randomClips.Count == 0)
            {
                return 1f;
            }

            float stateSpeed = state.speed != 0f ? Mathf.Abs(state.speed) : 1f;
            return GetSelectedOrMaxDuration(stateIndex, pickedIdx) / stateSpeed;
        }

        public override float GetUnscaledDuration(HonamiState state, int stateIndex, Dictionary<int, int> pickedIdx, float blendParam)
        {
            if (randomClips == null || randomClips.Count == 0)
            {
                return 1f;
            }

            return GetSelectedOrMaxDuration(stateIndex, pickedIdx);
        }

        public override void UpdateRuntime(in HonamiExecutionContext ctx)
        {
            if (randomClips == null || ctx.Playable.GetPlayableType() != typeof(AnimationMixerPlayable))
            {
                return;
            }

            AnimationMixerPlayable mixer = (AnimationMixerPlayable)ctx.Playable;

            for (int i = 0; i < randomClips.Count; i++)
            {
                Playable child = mixer.GetInput(i);
                if (!child.IsValid())
                {
                    continue;
                }

                HonamiRandomAnimationClip randomClip = randomClips[i];
                if (randomClip.clip == null)
                {
                    continue;
                }

                UpdateClipTime(child, randomClip, ctx);
            }
        }

        private static Playable CreateClipOutput(
            PlayableGraph graph,
            HonamiState state,
            HonamiRandomAnimationClip randomClip,
            Func<PlayableGraph, Playable> mirrorFactory)
        {
            AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(graph, randomClip.clip);
            clipPlayable.SetSpeed(randomClip.speed * (state.isReversed ? -state.speed : state.speed));
            clipPlayable.SetTime(state.isReversed ? GetStopTime(randomClip) : randomClip.startTime);
            clipPlayable.Play();

            if (state.loop)
            {
                clipPlayable.SetDuration(Mathf.Infinity);
            }

            if (!randomClip.mirror || mirrorFactory == null)
            {
                return clipPlayable;
            }

            Playable mirrorPlayable = mirrorFactory(graph);
            if (!mirrorPlayable.IsValid())
            {
                return clipPlayable;
            }

            mirrorPlayable.SetInputCount(1);
            graph.Connect(clipPlayable, 0, mirrorPlayable, 0);
            mirrorPlayable.SetInputWeight(0, 1f);
            return mirrorPlayable;
        }

        private float GetSelectedOrMaxDuration(int stateIndex, Dictionary<int, int> pickedIdx)
        {
            if (TryGetPickedClip(stateIndex, pickedIdx, out HonamiRandomAnimationClip pickedClip))
            {
                return GetClipDuration(pickedClip);
            }

            float maxDuration = 0f;
            foreach (HonamiRandomAnimationClip randomClip in randomClips)
            {
                if (randomClip.clip == null)
                {
                    continue;
                }

                float duration = GetClipDuration(randomClip);
                if (duration > maxDuration)
                {
                    maxDuration = duration;
                }
            }

            return maxDuration > 0f ? maxDuration : 1f;
        }

        private bool TryGetPickedClip(int stateIndex, Dictionary<int, int> pickedIdx, out HonamiRandomAnimationClip randomClip)
        {
            randomClip = null;

            if (pickedIdx == null || !pickedIdx.TryGetValue(stateIndex, out int picked))
            {
                return false;
            }

            if (picked < 0 || picked >= randomClips.Count || randomClips[picked].clip == null)
            {
                return false;
            }

            randomClip = randomClips[picked];
            return true;
        }

        private static void UpdateClipTime(Playable child, HonamiRandomAnimationClip randomClip, in HonamiExecutionContext ctx)
        {
            Playable actualClipPlayable = child;
            if (actualClipPlayable.GetPlayableType() == typeof(AnimationScriptPlayable) && actualClipPlayable.GetInputCount() > 0)
            {
                actualClipPlayable = actualClipPlayable.GetInput(0);
            }

            float stopTime = GetStopTime(randomClip);
            float duration = Mathf.Max(0.001f, stopTime - randomClip.startTime);
            double rawTime = ctx.Playable.GetTime() * GetClipSpeed(randomClip);
            rawTime = NormalizeTime(rawTime, duration, ctx.State.loop);

            double sampleTime = ctx.State.isReversed
                ? stopTime - rawTime
                : randomClip.startTime + rawTime;

            if (sampleTime >= randomClip.clip.length)
            {
                sampleTime = randomClip.clip.length - 0.001;
            }

            actualClipPlayable.SetTime(sampleTime);
            actualClipPlayable.SetSpeed(0f);

            if (actualClipPlayable.GetHandle() != child.GetHandle())
            {
                child.SetSpeed(0f);
            }
        }

        private static float GetClipDuration(HonamiRandomAnimationClip randomClip)
        {
            return Mathf.Max(0f, GetStopTime(randomClip) - randomClip.startTime) / GetClipSpeed(randomClip);
        }

        private static float GetStopTime(HonamiRandomAnimationClip randomClip)
        {
            return randomClip.endTime > 0 ? randomClip.endTime : randomClip.clip.length;
        }

        private static float GetClipSpeed(HonamiRandomAnimationClip randomClip)
        {
            return Mathf.Abs(randomClip.speed != 0f ? randomClip.speed : 1f);
        }

        private static double NormalizeTime(double time, float duration, bool loop)
        {
            if (loop && duration > 0)
            {
                double previousTime = time;
                time %= duration;
                return previousTime > 0.0 && time < 1e-4 ? duration : time;
            }

            return Math.Max(0.0, Math.Min(time, duration));
        }
    }
}
