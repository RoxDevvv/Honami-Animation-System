using UnityEngine;
using UnityEditor;

namespace HonamiAnimationSystem.Editor
{
    public static class HonamiEditorIcons
    {
        public static Texture2D BlendTree => GetIcon("HonamiBlendtree");
        public static Texture2D BlendTreeWhite => GetIcon("HonamiBlendtreeWhite");
        public static Texture2D Controller => GetIcon("HonamiController");
        public static Texture2D Graph => GetIcon("HonamiGraph");
        public static Texture2D GraphWhite => GetIcon("HonamiGraphWhite");
        public static Texture2D Timeline => GetIcon("HonamiTimeline");
        public static Texture2D TimelineWhite => GetIcon("HonamiTimelineWhite");
        public static Texture2D Profile => GetIcon("HonamiProfile");

        public static Texture2D GetIcon(string name) => Resources.Load<Texture2D>($"Icons/{name}");

        public static GUIContent IconContent(string name, string text = "") => new(text, GetIcon(name));
    }
}
