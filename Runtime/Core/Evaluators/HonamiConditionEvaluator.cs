using System.Collections.Generic;
using UnityEngine;

namespace HonamiAnimationSystem.Runtime.Core
{
    public static class HonamiConditionEvaluator
    {
        public static bool EvaluateConditionTentative(HonamiCondition cond, HonamiParameterStore paramStore, int paramHash, List<int> triggersTentative)
        {
            switch (cond.mode)
            {
                case HonamiConditionMode.If:
                    if (paramStore.ContainsBool(paramHash) && paramStore.IsBoolSet(paramHash)) return true;
                    if (paramStore.ContainsTrigger(paramHash) && paramStore.IsTriggerSet(paramHash))
                    {
                        if (!triggersTentative.Contains(paramHash)) triggersTentative.Add(paramHash);
                        return true;
                    }
                    return false;

                case HonamiConditionMode.IfNot:
                    if (paramStore.ContainsBool(paramHash)) return !paramStore.IsBoolSet(paramHash);
                    if (paramStore.ContainsTrigger(paramHash)) return !paramStore.IsTriggerSet(paramHash);
                    return false;

                case HonamiConditionMode.Greater:
                    if (paramStore.ContainsFloat(paramHash)) return paramStore.GetFloat(paramHash) > cond.threshold;
                    if (paramStore.ContainsInt(paramHash)) return paramStore.GetInteger(paramHash) > (int)cond.threshold;
                    break;

                case HonamiConditionMode.Less:
                    if (paramStore.ContainsFloat(paramHash)) return paramStore.GetFloat(paramHash) < cond.threshold;
                    if (paramStore.ContainsInt(paramHash)) return paramStore.GetInteger(paramHash) < (int)cond.threshold;
                    break;

                case HonamiConditionMode.Equals:
                    if (paramStore.ContainsFloat(paramHash)) return Mathf.Approximately(paramStore.GetFloat(paramHash), cond.threshold);
                    if (paramStore.ContainsInt(paramHash)) return paramStore.GetInteger(paramHash) == (int)cond.threshold;
                    break;

                case HonamiConditionMode.NotEqual:
                    if (paramStore.ContainsFloat(paramHash)) return !Mathf.Approximately(paramStore.GetFloat(paramHash), cond.threshold);
                    if (paramStore.ContainsInt(paramHash)) return paramStore.GetInteger(paramHash) != (int)cond.threshold;
                    break;
            }
            return false;
        }

        public static void ApplyParameterAssignments(HonamiState state, bool onExit, HonamiParameterStore paramStore, Dictionary<HonamiParameterAssignment, int> assignmentParamHashes)
        {
            if (state?.parameterAssignments == null) return;
            int count = state.parameterAssignments.Count;
            for (int i = 0; i < count; i++)
            {
                var assignment = state.parameterAssignments[i];
                if (assignment.executeOnExit == onExit)
                    paramStore.ApplyAssignment(assignment, assignmentParamHashes);
            }
        }

        public static void ApplyTransitionAssignments(HonamiTransition transition, HonamiParameterStore paramStore, Dictionary<HonamiParameterAssignment, int> assignmentParamHashes)
        {
            if (transition?.parameterAssignments == null) return;
            int count = transition.parameterAssignments.Count;
            for (int i = 0; i < count; i++)
                paramStore.ApplyAssignment(transition.parameterAssignments[i], assignmentParamHashes);
        }
    }
}
