using System.Collections.Generic;
using UnityEngine;

namespace HonamiAnimationSystem.Runtime.Core
{
    [CreateAssetMenu(fileName = "NewProfileState", menuName = "Honami Animation/Profiles/Profile State")]
    public sealed class HonamiProfileState : ScriptableObject
    {
        public string stateName;
        public HonamiRuntimeController controller;

        [Header("Transition Settings")]
        public float transitionDuration = 0f;
        public AnimationCurve transitionCurve = null;
        public HonamiControllerTransitionMode transitionMode = HonamiControllerTransitionMode.ContinueEvaluating;

        [HideInInspector] public string guid;
        [HideInInspector] public string parentGuid;
        [HideInInspector] public List<string> childrenGuids = new();
        [HideInInspector] public bool isExpanded = true;
        [HideInInspector] public Vector2 editorPosition;

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(guid)) guid = System.Guid.NewGuid().ToString();
        }
    }
}
