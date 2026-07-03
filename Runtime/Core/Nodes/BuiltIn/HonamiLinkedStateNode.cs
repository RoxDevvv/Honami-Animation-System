using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace HonamiAnimationSystem.Runtime.Core
{
    /// <summary>
    /// Legacy virtual node that broadcasts linked animator actions on state enter and exit.
    /// </summary>
    [CreateAssetMenu(fileName = "LinkedStateNode", menuName = "Honami Animation/Nodes/Linked State Node")]
    [System.Obsolete("Use HonamiLinkedAnimator.SetActionID or HonamiLinkedAction.Play instead.")]
    public sealed class HonamiLinkedStateNode : HonamiNodeBase
    {
        [Header("Broadcast Settings")]
        [Tooltip("Action ID to broadcast when entering this macro state.")]
        public HonamiActionID onEnterBroadcastAction;

        [Tooltip("Action ID to broadcast when exiting this macro state.")]
        public HonamiActionID onExitBroadcastAction;

        [Header("Targeting Options")]
        [Tooltip("Defines who receives the broadcast.")]
        public HonamiBroadcastTargetMode targetMode = HonamiBroadcastTargetMode.AllLinked;

        [Tooltip("Used only if Target Mode is 'ByTag'. Only animators with this linking tag will react.")]
        public HonamiTagID targetTag;

        public override bool IsVirtual => true;

        public override Playable CreatePlayable(PlayableGraph graph, HonamiState state)
        {
            return AnimationMixerPlayable.Create(graph, 0);
        }

        public override float GetDuration(HonamiState state, int stateIndex, Dictionary<int, int> pickedIdx, float blendParam)
        {
            return 1f;
        }

        public override void UpdateRuntime(in HonamiExecutionContext ctx)
        {
        }

        public override void OnEnter(in HonamiExecutionContext ctx)
        {
            base.OnEnter(ctx);
            BroadcastIfPossible(ctx, onEnterBroadcastAction);
        }

        public override void OnExit(in HonamiExecutionContext ctx)
        {
            base.OnExit(ctx);
            BroadcastIfPossible(ctx, onExitBroadcastAction);
        }

        private void BroadcastIfPossible(in HonamiExecutionContext ctx, HonamiActionID action)
        {
            if (action == null || ctx.Animator == null)
            {
                return;
            }

            var brain = ctx.Animator.GetComponentInParent<HonamiLinkedAnimator>();
            if (brain != null)
            {
                brain.BroadcastAction(action, targetMode, targetTag);
            }
        }
    }
}
