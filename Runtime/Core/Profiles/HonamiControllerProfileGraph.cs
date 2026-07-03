using System.Collections.Generic;
using UnityEngine;

namespace HonamiAnimationSystem.Runtime.Core
{
    [CreateAssetMenu(fileName = "NewProfileGraph", menuName = "Honami Animation/Profiles/Profile Graph")]
    public sealed class HonamiControllerProfileGraph : ScriptableObject
    {
        public List<HonamiProfileState> states = new();
        public HonamiProfileState defaultState;

        [HideInInspector] public string editorGraphDataJson;
        [HideInInspector] public List<HonamiGroupData> groups = new();
        [HideInInspector] public List<HonamiStickyNoteData> stickyNotes = new();

        public HonamiProfileState GetState(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            
            foreach (var state in states)
            {
                if (state != null && state.stateName == name) return state;
            }
            return null;
        }

        public HonamiProfileState GetStateByController(HonamiRuntimeController controller)
        {
            if (controller == null) return null;

            foreach (var state in states)
            {
                if (state != null && state.controller == controller) return state;
            }
            return null;
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (HonamiAssetImportGuard.IsImportingAssets) return;
            states?.RemoveAll(s => s == null);
#endif
        }
    }
}
