using UnityEngine;

namespace HonamiAnimationSystem.Runtime.Core
{
    [CreateAssetMenu(fileName = "BroadcastAction", menuName = "Honami Animation/Linked Animator Nodes/Broadcast Action")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom("BrainBroadcastActionNode")]
    public sealed class LinkedAnimatorBroadcastActionNode : HonamiLinkedAnimatorNodeBase
    {
        public HonamiActionID actionId;
        public float transitionDuration = 0.25f;
        public HonamiBroadcastTargetMode targetMode = HonamiBroadcastTargetMode.AllLinked;
        public HonamiTagID targetTag;

        public override LinkedAnimatorNodeResult Execute(HonamiLinkedAnimatorContext ctx)
        {
            if (actionId == null || ctx.Brain == null) return LinkedAnimatorNodeResult.Done;

            if (targetMode == HonamiBroadcastTargetMode.AllLinked)
                ctx.Brain.SetActionID(actionId, transitionDuration);
            else
                ctx.Brain.SetActionID(actionId, transitionDuration, targetMode, targetTag);

            return LinkedAnimatorNodeResult.Done;
        }
    }
}




