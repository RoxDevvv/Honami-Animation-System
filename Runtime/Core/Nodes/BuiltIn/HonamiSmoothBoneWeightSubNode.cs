using System;
using System.Collections.Generic;
using UnityEngine;

namespace HonamiAnimationSystem.Runtime.Core
{
    /// <summary>
    /// Sub-node that smooths one avatar mask bone weight from a start value back to its original mask weight.
    /// </summary>
    [CreateAssetMenu(fileName = "SmoothBoneWeightSubNode", menuName = "Honami Animation/Sub Nodes/Smooth Bone Weight")]
    public sealed class HonamiSmoothBoneWeightSubNode : HonamiSubNodeBase
    {
        [Tooltip("The path of the bone (e.g., root.x) for which the mask weight will be smoothly adjusted.")]
        public string bonePath = "root.x";

        [Tooltip("The initial mask weight of the bone when entering the state.")]
        [Range(0f, 1f)]
        public float startWeight = 0f;

        [Tooltip("The time (in seconds) it takes to smoothly reach the original mask weight.")]
        public float duration = 0.25f;

        [Tooltip("Should the bone weight be restored to its original value when exiting this state?")]
        public bool restoreOnExit = true;

        [NonSerialized]
        private Dictionary<int, RuntimeData> _data;

        public override void OnEnter(in HonamiExecutionContext ctx)
        {
            var mask = ctx.State.avatarMask;
            if (mask == null || ctx.Animator == null)
            {
                return;
            }

            _data ??= new Dictionary<int, RuntimeData>();
            RuntimeData data = GetOrCreateRuntimeData(ctx, mask);
            if (data.atlasIndex == -1)
            {
                return;
            }

            var atlas = ctx.Animator._avatarProcessor.GetMaskAtlas();
            if (atlas.IsCreated)
            {
                data.originalMaskWeight = mask.GetWeight(data.fullPathHash);
                data.currentWeight = startWeight;
                atlas[data.atlasIndex] = startWeight;
            }
        }

        public override void UpdateRuntime(in HonamiExecutionContext ctx)
        {
            if (!TryGetRuntimeData(ctx.StateIndex, out RuntimeData data))
            {
                return;
            }

            if (data.currentWeight == data.originalMaskWeight)
            {
                return;
            }

            float step = duration > 0f ? (float)(ctx.DeltaTime / duration) : 1f;
            data.currentWeight = Mathf.MoveTowards(data.currentWeight, data.originalMaskWeight, step);

            var atlas = ctx.Animator._avatarProcessor.GetMaskAtlas();
            if (atlas.IsCreated)
            {
                atlas[data.atlasIndex] = data.currentWeight;
            }
        }

        public override void OnExit(in HonamiExecutionContext ctx)
        {
            if (!restoreOnExit || !TryGetRuntimeData(ctx.StateIndex, out RuntimeData data))
            {
                return;
            }

            var atlas = ctx.Animator._avatarProcessor.GetMaskAtlas();
            if (atlas.IsCreated)
            {
                atlas[data.atlasIndex] = data.originalMaskWeight;
            }
        }

        private RuntimeData GetOrCreateRuntimeData(in HonamiExecutionContext ctx, HonamiAvatarMask mask)
        {
            if (_data.TryGetValue(ctx.StateIndex, out RuntimeData data))
            {
                return data;
            }

            data = new RuntimeData { atlasIndex = -1 };

            if (ctx.Animator._avatarProcessor != null &&
                ctx.Animator._avatarProcessor.TryGetMaskBoneAtlasIndex(mask, bonePath, out int atlasIndex, out string fullPath))
            {
                data.atlasIndex = atlasIndex;
                data.fullPathHash = fullPath.GetHashCode();
            }

            _data[ctx.StateIndex] = data;
            return data;
        }

        private bool TryGetRuntimeData(int stateIndex, out RuntimeData data)
        {
            data = null;
            return _data != null &&
                   _data.TryGetValue(stateIndex, out data) &&
                   data.atlasIndex != -1;
        }

        private sealed class RuntimeData
        {
            public int atlasIndex;
            public int fullPathHash;
            public float currentWeight;
            public float originalMaskWeight;
        }
    }
}
