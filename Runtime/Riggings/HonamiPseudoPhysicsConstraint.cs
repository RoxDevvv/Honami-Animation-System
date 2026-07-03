using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace HonamiAnimationSystem.Runtime.Riggings
{
    [Serializable]
    public sealed class HonamiPhysicsBoneData
    {
        public Transform bone;
        [Range(0f, 1f)] public float weightMultiplier = 1f;

        internal Vector3 currentPosOffset;
        internal Vector3 currentPosVelocity;
        internal Vector3 currentRotOffset;
        internal Vector3 currentRotVelocity;

        internal Vector3 lastWorldPos;
        internal Quaternion lastWorldRot;
        internal Vector3 lastVelocity;
        internal Vector3 lastAngularVelocity;
    }

    [BurstCompile]
    public struct HonamiPseudoPhysicsJob : IAnimationJob
    {
        public NativeArray<TransformStreamHandle> boneHandles;

        [ReadOnly] public NativeArray<float3> positionOffsets;
        [ReadOnly] public NativeArray<float3> rotationOffsets;
        [ReadOnly] public NativeArray<float> boneWeights;
        [ReadOnly] public NativeArray<float3> posAxisMask;
        [ReadOnly] public NativeArray<float3> rotAxisMask;
        [ReadOnly] public NativeArray<float> parameters;

        private const int ParamGlobalWeight = 0;
        private const int ParamBoneCount = 1;

        public void ProcessAnimation(AnimationStream stream)
        {
            float gw = parameters[ParamGlobalWeight];
            if (gw <= 0.001f) return;

            int count = (int)parameters[ParamBoneCount];
            float3 posMask = posAxisMask[0];
            float3 rotMask = rotAxisMask[0];

            for (int i = 0; i < count; i++)
            {
                float w = boneWeights[i] * gw;
                if (w <= 0.001f) continue;

                float3 finalPosOffset = positionOffsets[i] * posMask * w;
                float3 finalRotOffset = rotationOffsets[i] * rotMask * w;

                float3 currentPos = (float3)boneHandles[i].GetPosition(stream);
                quaternion currentRot = (quaternion)boneHandles[i].GetRotation(stream);

                boneHandles[i].SetPosition(stream, (Vector3)(currentPos + math.mul(currentRot, finalPosOffset)));
                boneHandles[i].SetRotation(stream, (Quaternion)math.mul(currentRot, quaternion.Euler(math.radians(finalRotOffset))));
            }
        }

        public void ProcessRootMotion(AnimationStream stream) { }
    }

    [AddComponentMenu("Honami Animation/Riggings/Honami Pseudo-Physics Constraint")]
    public sealed class HonamiPseudoPhysicsConstraint : HonamiRig
    {
        [Header("Target Bones")]
        public HonamiPhysicsBoneData[] bones;

        [Header("Axis Setup")]
        public Vector3 positionAxisMask = Vector3.one;
        public Vector3 rotationAxisMask = Vector3.one;

        [Header("Position Physics")]
        public Vector3 positionDrag = new Vector3(0.05f, 0.05f, 0.05f);
        public Vector3 positionInertia = new Vector3(0.1f, 0.1f, 0.1f);
        public float positionStiffness = 50f;
        public float positionDamping = 5f;
        public Vector3 maxPositionOffset = new Vector3(0.5f, 0.5f, 0.5f);
        public Vector3 positionLocalGravity = Vector3.zero;

        [Header("Rotation Physics")]
        public Vector3 rotationDrag = new Vector3(2f, 2f, 2f);
        public Vector3 rotationInertia = new Vector3(5f, 5f, 5f);
        public float rotationStiffness = 50f;
        public float rotationDamping = 5f;
        public Vector3 maxRotationOffset = new Vector3(45f, 45f, 45f);

        [Header("Cross Effects")]
        public Vector3 movementToRotation = Vector3.zero;

        private bool _isInitialized;

        private AnimationScriptPlayable _playable;
        private NativeArray<TransformStreamHandle> _boneHandles;
        private NativeArray<float3> _nativePosOffsets;
        private NativeArray<float3> _nativeRotOffsets;
        private NativeArray<float> _nativeBoneWeights;
        private NativeArray<float3> _nativePosMask;
        private NativeArray<float3> _nativeRotMask;
        private NativeArray<float> _nativeParams;
        private int _boundBoneCount;

        public override void ResetRig()
        {
            _isInitialized = false;
        }

        public override Playable CreatePlayable(Animator animator, PlayableGraph graph)
        {
            DisposeJobData();
            if (bones == null || bones.Length == 0) return Playable.Null;

            int validCount = 0;
            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i] != null && bones[i].bone != null) validCount++;
            }
            if (validCount == 0) return Playable.Null;

            _boundBoneCount = validCount;

            _boneHandles = new NativeArray<TransformStreamHandle>(validCount, Allocator.Persistent);
            _nativePosOffsets = new NativeArray<float3>(validCount, Allocator.Persistent);
            _nativeRotOffsets = new NativeArray<float3>(validCount, Allocator.Persistent);
            _nativeBoneWeights = new NativeArray<float>(validCount, Allocator.Persistent);
            _nativePosMask = new NativeArray<float3>(1, Allocator.Persistent);
            _nativeRotMask = new NativeArray<float3>(1, Allocator.Persistent);
            _nativeParams = new NativeArray<float>(2, Allocator.Persistent);

            int idx = 0;
            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i] == null || bones[i].bone == null) continue;
                _boneHandles[idx] = animator.BindStreamTransform(bones[i].bone);
                idx++;
            }

            var job = new HonamiPseudoPhysicsJob
            {
                boneHandles = _boneHandles,
                positionOffsets = _nativePosOffsets,
                rotationOffsets = _nativeRotOffsets,
                boneWeights = _nativeBoneWeights,
                posAxisMask = _nativePosMask,
                rotAxisMask = _nativeRotMask,
                parameters = _nativeParams
            };

            _playable = AnimationScriptPlayable.Create(graph, job, 1);
            return _playable;
        }

        public override void PrepareJobData(float deltaTime)
        {
            if (!_playable.IsValid() || !Application.isPlaying)
            {
                if (_nativeParams.IsCreated) _nativeParams[0] = 0f;
                _isInitialized = false;
                return;
            }

            if (bones == null || bones.Length == 0 || weight <= 0.001f)
            {
                if (_nativeParams.IsCreated) _nativeParams[0] = 0f;
                _isInitialized = false;
                return;
            }

            if (deltaTime <= 0.0001f)
            {
                if (_nativeParams.IsCreated) _nativeParams[0] = 0f;
                return;
            }

            float dt = deltaTime;

            if (!_isInitialized)
            {
                InitializeBones();
                _isInitialized = true;
                if (_nativeParams.IsCreated) _nativeParams[0] = 0f;
                return;
            }

            _nativePosMask[0] = positionAxisMask;
            _nativeRotMask[0] = rotationAxisMask;

            int idx = 0;
            for (int i = 0; i < bones.Length; i++)
            {
                var b = bones[i];
                if (b == null || b.bone == null) continue;

                Vector3 currentWorldPos = b.bone.position;
                Quaternion currentWorldRot = b.bone.rotation;

                Vector3 deltaPos = currentWorldPos - b.lastWorldPos;
                Vector3 velocity = deltaPos / dt;
                Vector3 acceleration = (velocity - b.lastVelocity) / dt;

                Quaternion deltaRot = currentWorldRot * Quaternion.Inverse(b.lastWorldRot);
                deltaRot.ToAngleAxis(out float angle, out Vector3 axis);
                if (angle > 180f) angle -= 360f;

                Vector3 angularVelocity = Vector3.zero;
                if (angle != 0f && axis.sqrMagnitude > 0f)
                    angularVelocity = axis.normalized * (angle / dt);
                Vector3 angularAcceleration = (angularVelocity - b.lastAngularVelocity) / dt;

                Vector3 localVelocity = Quaternion.Inverse(currentWorldRot) * velocity;
                Vector3 localAcceleration = Quaternion.Inverse(currentWorldRot) * acceleration;
                Vector3 localAngVelocity = Quaternion.Inverse(currentWorldRot) * angularVelocity;
                Vector3 localAngAcceleration = Quaternion.Inverse(currentWorldRot) * angularAcceleration;

                b.currentPosOffset -= Vector3.Scale(localVelocity, positionDrag) * dt;
                b.currentPosVelocity -= Vector3.Scale(localAcceleration, positionInertia) * dt;

                b.currentRotOffset -= Vector3.Scale(localAngVelocity, rotationDrag) * dt;
                b.currentRotVelocity -= Vector3.Scale(localAngAcceleration, rotationInertia) * dt;

                b.currentRotOffset -= Vector3.Scale(localVelocity, movementToRotation) * dt;

                Vector3 posForce = -b.currentPosOffset * positionStiffness + positionLocalGravity;
                b.currentPosVelocity += posForce * dt;
                b.currentPosVelocity -= b.currentPosVelocity * Mathf.Clamp01(positionDamping * dt);
                b.currentPosOffset += b.currentPosVelocity * dt;

                Vector3 rotForce = -b.currentRotOffset * rotationStiffness;
                b.currentRotVelocity += rotForce * dt;
                b.currentRotVelocity -= b.currentRotVelocity * Mathf.Clamp01(rotationDamping * dt);
                b.currentRotOffset += b.currentRotVelocity * dt;

                b.currentPosOffset = ClampVector(b.currentPosOffset, maxPositionOffset);
                b.currentRotOffset = ClampVector(b.currentRotOffset, maxRotationOffset);

                _nativePosOffsets[idx] = b.currentPosOffset;
                _nativeRotOffsets[idx] = b.currentRotOffset;
                _nativeBoneWeights[idx] = b.weightMultiplier;

                b.lastWorldPos = currentWorldPos;
                b.lastWorldRot = currentWorldRot;
                b.lastVelocity = velocity;
                b.lastAngularVelocity = angularVelocity;

                idx++;
            }

            _nativeParams[0] = weight;
            _nativeParams[1] = _boundBoneCount;
        }

        public override void DisposeJobData()
        {
            if (_boneHandles.IsCreated) _boneHandles.Dispose();
            if (_nativePosOffsets.IsCreated) _nativePosOffsets.Dispose();
            if (_nativeRotOffsets.IsCreated) _nativeRotOffsets.Dispose();
            if (_nativeBoneWeights.IsCreated) _nativeBoneWeights.Dispose();
            if (_nativePosMask.IsCreated) _nativePosMask.Dispose();
            if (_nativeRotMask.IsCreated) _nativeRotMask.Dispose();
            if (_nativeParams.IsCreated) _nativeParams.Dispose();
        }

        private void InitializeBones()
        {
            if (bones == null) return;
            for (int i = 0; i < bones.Length; i++)
            {
                var b = bones[i];
                if (b == null || b.bone == null) continue;
                b.lastWorldPos = b.bone.position;
                b.lastWorldRot = b.bone.rotation;
                b.lastVelocity = Vector3.zero;
                b.lastAngularVelocity = Vector3.zero;
                b.currentPosOffset = Vector3.zero;
                b.currentPosVelocity = Vector3.zero;
                b.currentRotOffset = Vector3.zero;
                b.currentRotVelocity = Vector3.zero;
            }
        }

        private Vector3 ClampVector(Vector3 v, Vector3 max)
        {
            return new Vector3(
                Mathf.Clamp(v.x, -Mathf.Abs(max.x), Mathf.Abs(max.x)),
                Mathf.Clamp(v.y, -Mathf.Abs(max.y), Mathf.Abs(max.y)),
                Mathf.Clamp(v.z, -Mathf.Abs(max.z), Mathf.Abs(max.z))
            );
        }

        public override void ProcessRig(float deltaTime)
        {
            if (!Application.isPlaying)
            {
                _isInitialized = false;
                return;
            }

            if (bones == null || bones.Length == 0 || weight <= 0.001f)
            {
                _isInitialized = false;
                return;
            }

            if (deltaTime <= 0.0001f) return;
            float dt = deltaTime;

            if (!_isInitialized)
            {
                InitializeBones();
                _isInitialized = true;
                return;
            }

            foreach (var b in bones)
            {
                if (b.bone == null) continue;

                Vector3 currentWorldPos = b.bone.position;
                Quaternion currentWorldRot = b.bone.rotation;

                Vector3 delta = currentWorldPos - b.lastWorldPos;
                Vector3 velocity = delta / dt;
                Vector3 acceleration = (velocity - b.lastVelocity) / dt;

                Quaternion deltaRot = currentWorldRot * Quaternion.Inverse(b.lastWorldRot);
                deltaRot.ToAngleAxis(out float angle, out Vector3 axis);
                if (angle > 180f) angle -= 360f;

                Vector3 angularVelocity = Vector3.zero;
                if (angle != 0f && axis.sqrMagnitude > 0f)
                    angularVelocity = axis.normalized * (angle / dt);
                Vector3 angularAcceleration = (angularVelocity - b.lastAngularVelocity) / dt;

                Vector3 localVelocity = Quaternion.Inverse(currentWorldRot) * velocity;
                Vector3 localAcceleration = Quaternion.Inverse(currentWorldRot) * acceleration;
                Vector3 localAngVelocity = Quaternion.Inverse(currentWorldRot) * angularVelocity;
                Vector3 localAngAcceleration = Quaternion.Inverse(currentWorldRot) * angularAcceleration;

                b.currentPosOffset -= Vector3.Scale(localVelocity, positionDrag) * dt;
                b.currentPosVelocity -= Vector3.Scale(localAcceleration, positionInertia) * dt;
                b.currentRotOffset -= Vector3.Scale(localAngVelocity, rotationDrag) * dt;
                b.currentRotVelocity -= Vector3.Scale(localAngAcceleration, rotationInertia) * dt;
                b.currentRotOffset -= Vector3.Scale(localVelocity, movementToRotation) * dt;

                Vector3 posForce = -b.currentPosOffset * positionStiffness + positionLocalGravity;
                b.currentPosVelocity += posForce * dt;
                b.currentPosVelocity -= b.currentPosVelocity * Mathf.Clamp01(positionDamping * dt);
                b.currentPosOffset += b.currentPosVelocity * dt;

                Vector3 rotForce = -b.currentRotOffset * rotationStiffness;
                b.currentRotVelocity += rotForce * dt;
                b.currentRotVelocity -= b.currentRotVelocity * Mathf.Clamp01(rotationDamping * dt);
                b.currentRotOffset += b.currentRotVelocity * dt;

                b.currentPosOffset = ClampVector(b.currentPosOffset, maxPositionOffset);
                b.currentRotOffset = ClampVector(b.currentRotOffset, maxRotationOffset);

                Vector3 finalPosOffset = Vector3.Scale(b.currentPosOffset, positionAxisMask) * weight * b.weightMultiplier;
                Vector3 finalRotOffset = Vector3.Scale(b.currentRotOffset, rotationAxisMask) * weight * b.weightMultiplier;

                b.bone.position = currentWorldPos + currentWorldRot * finalPosOffset;
                b.bone.rotation = currentWorldRot * Quaternion.Euler(finalRotOffset);

                b.lastWorldPos = currentWorldPos;
                b.lastWorldRot = currentWorldRot;
                b.lastVelocity = velocity;
                b.lastAngularVelocity = angularVelocity;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (bones == null) return;
            foreach (var b in bones)
            {
                if (b.bone == null) continue;
                Gizmos.color = Color.cyan * new Color(1, 1, 1, weight * b.weightMultiplier);
                Gizmos.DrawWireSphere(b.bone.position, 0.02f);
            }
        }
#endif
    }
}
