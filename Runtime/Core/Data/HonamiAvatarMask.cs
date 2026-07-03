using System;
using System.Collections.Generic;
using UnityEngine;

namespace HonamiAnimationSystem.Runtime.Core
{
    /// <summary>
    /// Defines per-bone weights used to mask Honami state, layer, or sub-node output.
    /// </summary>
    [CreateAssetMenu(fileName = "NewHonamiAvatarMask", menuName = "Honami Animation/Avatar Mask")]
    public sealed class HonamiAvatarMask : ScriptableObject
    {
        /// <summary>
        /// Serialized mask weight for a single avatar bone path.
        /// </summary>
        [Serializable]
        public sealed class BoneWeight
        {
            public string bonePath;

            [Range(0f, 1f)]
            public float weight = 1f;
        }

        public HonamiAvatar avatar;
        public List<BoneWeight> boneWeights = new();

        [Tooltip("If true, bones NOT listed in boneWeights are fully blocked (weight = 0).\n" +
                 "If false (default), unlisted bones pass through at full weight (weight = 1).")]
        public bool excludeUnlisted = false;

        private Dictionary<int, float> _weightCache;

        /// <summary>
        /// Gets the mask weight for a bone path hash.
        /// </summary>
        public float GetWeight(int pathHash)
        {
            EnsureWeightCache();
            return _weightCache.TryGetValue(pathHash, out float weight)
                ? weight
                : (excludeUnlisted ? 0f : 1f);
        }

        private void OnEnable() => _weightCache = null;

        private void OnValidate() => _weightCache = null;

        /// <summary>
        /// Invalidates the weight cache so the next <see cref="GetWeight"/> call rebuilds it.
        /// </summary>
        public void InvalidateCache() => _weightCache = null;

        /// <summary>
        /// Rebuilds mask entries from the assigned avatar while preserving existing weights by bone path.
        /// </summary>
        public void SyncWithAvatar()
        {
            if (avatar == null)
            {
                return;
            }

            var existing = new Dictionary<string, float>();
            for (int i = 0; i < boneWeights.Count; i++)
            {
                existing[boneWeights[i].bonePath] = boneWeights[i].weight;
            }

            boneWeights.Clear();
            for (int i = 0; i < avatar.bones.Count; i++)
            {
                var entry = avatar.bones[i];
                if (!entry.enabled)
                {
                    continue;
                }

                boneWeights.Add(new BoneWeight
                {
                    bonePath = entry.bonePath,
                    weight = existing.TryGetValue(entry.bonePath, out float weight) ? weight : 1f
                });
            }

            _weightCache = null;
        }

        private void EnsureWeightCache()
        {
            if (_weightCache != null)
            {
                return;
            }

            _weightCache = new Dictionary<int, float>(boneWeights.Count);
            for (int i = 0; i < boneWeights.Count; i++)
            {
                _weightCache[boneWeights[i].bonePath.GetHashCode()] = boneWeights[i].weight;
            }
        }
    }
}
