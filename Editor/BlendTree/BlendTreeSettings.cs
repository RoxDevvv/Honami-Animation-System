#if UNITY_EDITOR
using UnityEditor;

namespace HonamiAnimationSystem.Editor.BlendTree
{
    internal static class BlendTreeSettings
    {
        private const string Prefix = "Honami_BlendTree_";
        private const string ShowPropertiesKey = Prefix + "ShowProperties";
        private const string PropsWidthKey = Prefix + "PropsWidth";

        public static bool ShowProperties
        {
            get => EditorPrefs.GetBool(ShowPropertiesKey, true);
            set => EditorPrefs.SetBool(ShowPropertiesKey, value);
        }

        public static float PropsWidth
        {
            get => EditorPrefs.GetFloat(PropsWidthKey, BlendTreeTheme.PropsWidth);
            set => EditorPrefs.SetFloat(PropsWidthKey, value);
        }
    }
}
#endif
