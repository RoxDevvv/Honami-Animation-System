using UnityEditor;

namespace HonamiAnimationSystem.Editor
{
    public static class HonamiGraphSettings
    {
        private const string MinimapKey = "Honami_ShowMinimap";
        private const string AnimationsKey = "Honami_EnableAnimations";
        private const string ShowGridKey = "Honami_ShowGrid";
        private const string Tactile3DKey = "Honami_Tactile3D";

        public static bool ShowMinimap
        {
            get => EditorPrefs.GetBool(MinimapKey, true);
            set => EditorPrefs.SetBool(MinimapKey, value);
        }

        public static bool EnableAnimations
        {
            get => EditorPrefs.GetBool(AnimationsKey, true);
            set => EditorPrefs.SetBool(AnimationsKey, value);
        }

        public static bool ShowGrid
        {
            get => EditorPrefs.GetBool(ShowGridKey, true);
            set => EditorPrefs.SetBool(ShowGridKey, value);
        }

        public static bool EnableTactile3D
        {
            get => EditorPrefs.GetBool(Tactile3DKey, true);
            set => EditorPrefs.SetBool(Tactile3DKey, value);
        }
    }
}
