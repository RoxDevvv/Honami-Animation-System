using System;
using System.Collections.Generic;
using UnityEngine;

namespace HonamiAnimationSystem.Runtime.Core
{
    /// <summary>
    /// Defines the bone list and mirror pairs used by Honami avatar processing.
    /// </summary>
    [CreateAssetMenu(fileName = "NewHonamiAvatar", menuName = "Honami Animation/Avatar")]
    public sealed class HonamiAvatar : ScriptableObject
    {
        /// <summary>
        /// Serialized bone entry tracked by a Honami avatar.
        /// </summary>
        [Serializable]
        public sealed class BoneEntry
        {
            public string boneName;
            public string bonePath;
            public bool enabled = true;
        }

        /// <summary>
        /// Serialized pair of mirrored bone paths.
        /// </summary>
        [Serializable]
        public sealed class MirrorEntry
        {
            public string boneA;
            public string boneB;
        }

        public List<BoneEntry> bones = new();
        public List<MirrorEntry> mirrorBones = new();

        /// <summary>
        /// Finds an avatar bone by transform path.
        /// </summary>
        public BoneEntry FindByPath(string path)
        {
            for (int i = 0; i < bones.Count; i++)
            {
                if (bones[i].bonePath == path)
                {
                    return bones[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the mirrored bone path for a path registered in <see cref="mirrorBones"/>.
        /// </summary>
        public string GetMirrorPath(string path)
        {
            for (int i = 0; i < mirrorBones.Count; i++)
            {
                if (mirrorBones[i].boneA == path)
                {
                    return mirrorBones[i].boneB;
                }

                if (mirrorBones[i].boneB == path)
                {
                    return mirrorBones[i].boneA;
                }
            }

            return null;
        }
    }
}
