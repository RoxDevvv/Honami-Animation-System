#if UNITY_EDITOR
using UnityEngine;

namespace HonamiAnimationSystem.Editor
{
    public static class HonamiEditorTheme
    {
        public static readonly Color Accent = new(1.00f, 0.278f, 0.522f, 1f);
        public static readonly Color AccentDim = new(0.72f, 0.16f, 0.36f, 1f);
        public static readonly Color AccentSoft = new(1.00f, 0.278f, 0.522f, 0.18f);

        public static readonly Color WindowBg = new(0.055f, 0.058f, 0.064f);
        public static readonly Color PanelBg = new(0.095f, 0.102f, 0.112f);
        public static readonly Color ToolbarBg = new(0.086f, 0.09f, 0.098f);
        public static readonly Color ToolbarButton = new(0.13f, 0.14f, 0.155f);
        public static readonly Color ToolbarButtonHot = new(0.18f, 0.19f, 0.21f);
        public static readonly Color ToolbarButtonPressed = new(0.08f, 0.085f, 0.095f);
        public static readonly Color BoxBg = new(0.128f, 0.138f, 0.152f);
        public static readonly Color Divider = new(0f, 0f, 0f, 0.58f);
        public static readonly Color SubtleLine = new(1f, 1f, 1f, 0.07f);
        public static readonly Color Text = new(0.88f, 0.90f, 0.92f);
        public static readonly Color MutedText = new(0.58f, 0.61f, 0.65f);
    }
}
#endif
