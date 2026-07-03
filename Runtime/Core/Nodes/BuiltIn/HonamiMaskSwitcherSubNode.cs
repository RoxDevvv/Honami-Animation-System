using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace HonamiAnimationSystem.Runtime.Core
{
    /// <summary>
    /// Conditional mask and mirror override rule used by <see cref="HonamiMaskSwitcherSubNode"/>.
    /// </summary>
    [Serializable]
    public sealed class HonamiMaskRule
    {
        public List<HonamiCondition> conditions = new();
        public HonamiAvatarMask mask;
        public bool mirror;
    }

    /// <summary>
    /// Sub-node that switches the active state avatar mask and mirror weight from runtime conditions.
    /// </summary>
    [CreateAssetMenu(fileName = "MaskSwitcherSubNode", menuName = "Honami Animation/Sub Nodes/Mask Switcher")]
    public sealed class HonamiMaskSwitcherSubNode : HonamiSubNodeBase
    {
        public List<HonamiMaskRule> rules = new();

        [NonSerialized] private bool _isInitialized;
        [NonSerialized] private HonamiAvatarMask _originalMask;
        [NonSerialized] private bool _originalMirror;
        [NonSerialized] private Dictionary<HonamiCondition, int> _conditionHashes;
        [NonSerialized] private List<int> _tempTriggers;

        public override bool RequiresMirroring => ContainsMirrorRule();

        public override void OnEnter(in HonamiExecutionContext ctx)
        {
            _originalMask = ctx.State.avatarMask;
            _originalMirror = ctx.State.mirror;

            if (_isInitialized)
            {
                return;
            }

            _conditionHashes ??= new Dictionary<HonamiCondition, int>();
            _tempTriggers ??= new List<int>();
            CacheConditionHashes();

            _isInitialized = true;
        }

        public override void UpdateRuntime(in HonamiExecutionContext ctx)
        {
            if (!_isInitialized)
            {
                return;
            }

            _tempTriggers.Clear();
            HonamiMaskRule activeRule = FindActiveRule(ctx);

            HonamiAvatarMask targetMask = activeRule != null ? activeRule.mask : _originalMask;
            bool targetMirror = activeRule != null ? activeRule.mirror : _originalMirror;

            if (ctx.State.avatarMask != targetMask)
            {
                ctx.State.avatarMask = targetMask;
            }

            ApplyMirrorWeight(ctx, targetMirror);
        }

        public override void OnExit(in HonamiExecutionContext ctx)
        {
            if (!_isInitialized)
            {
                return;
            }

            ctx.State.avatarMask = _originalMask;
            ApplyMirrorWeight(ctx, _originalMirror);
            _tempTriggers?.Clear();
        }

        public override void GatherMasks(HashSet<HonamiAvatarMask> masks)
        {
            if (rules == null)
            {
                return;
            }

            for (int i = 0; i < rules.Count; i++)
            {
                if (rules[i].mask != null)
                {
                    masks.Add(rules[i].mask);
                }
            }
        }

        private void CacheConditionHashes()
        {
            for (int i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];
                for (int j = 0; j < rule.conditions.Count; j++)
                {
                    var condition = rule.conditions[j];
                    if (!_conditionHashes.ContainsKey(condition))
                    {
                        _conditionHashes[condition] = HonamiAnimator.StringToHash(condition.parameter);
                    }
                }
            }
        }

        private HonamiMaskRule FindActiveRule(in HonamiExecutionContext ctx)
        {
            for (int i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];
                if (AreConditionsMet(rule, ctx))
                {
                    return rule;
                }
            }

            return null;
        }

        private bool AreConditionsMet(HonamiMaskRule rule, in HonamiExecutionContext ctx)
        {
            for (int i = 0; i < rule.conditions.Count; i++)
            {
                var condition = rule.conditions[i];
                if (!_conditionHashes.TryGetValue(condition, out int hash))
                {
                    hash = HonamiAnimator.StringToHash(condition.parameter);
                    _conditionHashes[condition] = hash;
                }

                if (!HonamiConditionEvaluator.EvaluateConditionTentative(condition, ctx.Params, hash, _tempTriggers))
                {
                    return false;
                }
            }

            return true;
        }

        private static void ApplyMirrorWeight(in HonamiExecutionContext ctx, bool mirror)
        {
            Playable rootPlayable = ctx.LayerMixer.GetInput(ctx.PortIndex);
            if (!rootPlayable.IsValid() || rootPlayable.GetPlayableType() != typeof(AnimationScriptPlayable))
            {
                return;
            }

            var scriptPlayable = (AnimationScriptPlayable)rootPlayable;
            var job = scriptPlayable.GetJobData<HonamiMirrorJob>();
            float targetWeight = mirror ? 1f : 0f;

            if (Mathf.Abs(job.weight - targetWeight) > 0.001f)
            {
                job.weight = targetWeight;
                scriptPlayable.SetJobData(job);
            }
        }

        private bool ContainsMirrorRule()
        {
            if (rules == null)
            {
                return false;
            }

            for (int i = 0; i < rules.Count; i++)
            {
                if (rules[i].mirror)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
