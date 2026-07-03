using System;
using System.Collections.Generic;
using UnityEngine;

namespace HonamiAnimationSystem.Runtime.Core
{
    /// <summary>
    /// Conditional state-property override rule used by <see cref="HonamiReplacerSubNode"/>.
    /// </summary>
    [Serializable]
    public sealed class HonamiReplacerRule
    {
        public List<HonamiCondition> conditions = new();

        public bool overrideSpeed;
        public float speed = 1f;

        public bool overrideLoop;
        public bool loop = true;

        public bool overrideIsReversed;
        public bool isReversed = false;

        public bool overrideWeight;

        [Range(0f, 1f)]
        public float weight = 1f;

        public bool overrideMask;
        public HonamiAvatarMask mask;
    }

    /// <summary>
    /// Sub-node that conditionally overrides basic state properties while the state is active.
    /// </summary>
    [CreateAssetMenu(fileName = "ReplacerSubNode", menuName = "Honami Animation/Sub Nodes/Replacer")]
    public sealed class HonamiReplacerSubNode : HonamiSubNodeBase
    {
        public List<HonamiReplacerRule> rules = new();

        [NonSerialized] private bool _isInitialized;
        [NonSerialized] private float _originalSpeed;
        [NonSerialized] private bool _originalLoop;
        [NonSerialized] private bool _originalIsReversed;
        [NonSerialized] private float _originalWeight;
        [NonSerialized] private HonamiAvatarMask _originalMask;
        [NonSerialized] private Dictionary<HonamiCondition, int> _conditionHashes;
        [NonSerialized] private List<int> _tempTriggers;

        public override void OnEnter(in HonamiExecutionContext ctx)
        {
            if (_isInitialized)
            {
                return;
            }

            CaptureOriginals(ctx);
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
            HonamiReplacerRule activeRule = FindActiveRule(ctx);

            if (activeRule != null)
            {
                ApplyRule(ctx, activeRule);
            }
            else
            {
                RestoreOriginals(ctx);
            }
        }

        public override void OnExit(in HonamiExecutionContext ctx)
        {
            RestoreOriginals(ctx);
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
                var rule = rules[i];
                if (rule.overrideMask && rule.mask != null)
                {
                    masks.Add(rule.mask);
                }
            }
        }

        private void CaptureOriginals(in HonamiExecutionContext ctx)
        {
            _originalSpeed = ctx.State.speed;
            _originalLoop = ctx.State.loop;
            _originalIsReversed = ctx.State.isReversed;
            _originalWeight = ctx.State.weight;
            _originalMask = ctx.State.avatarMask;
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

        private HonamiReplacerRule FindActiveRule(in HonamiExecutionContext ctx)
        {
            for (int i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];
                if (rule.conditions.Count > 0 && AreConditionsMet(rule, ctx))
                {
                    return rule;
                }
            }

            return null;
        }

        private bool AreConditionsMet(HonamiReplacerRule rule, in HonamiExecutionContext ctx)
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

        private void ApplyRule(in HonamiExecutionContext ctx, HonamiReplacerRule rule)
        {
            ctx.State.speed = rule.overrideSpeed ? rule.speed : _originalSpeed;
            ctx.State.loop = rule.overrideLoop ? rule.loop : _originalLoop;
            ctx.State.isReversed = rule.overrideIsReversed ? rule.isReversed : _originalIsReversed;
            ctx.State.weight = rule.overrideWeight ? rule.weight : _originalWeight;
            ctx.State.avatarMask = rule.overrideMask ? rule.mask : _originalMask;
        }

        private void RestoreOriginals(in HonamiExecutionContext ctx)
        {
            if (!_isInitialized)
            {
                return;
            }

            ctx.State.speed = _originalSpeed;
            ctx.State.loop = _originalLoop;
            ctx.State.isReversed = _originalIsReversed;
            ctx.State.weight = _originalWeight;
            ctx.State.avatarMask = _originalMask;
        }
    }
}
