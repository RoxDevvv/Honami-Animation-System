using UnityEditor;

namespace HonamiAnimationSystem.Editor.Documentation
{
    public enum HonamiDocLanguage
    {
        English,
        Ukrainian
    }

    public static class HonamiDocLocalization
    {
        private const string PrefKey = "Honami.Docs.Language";

        public static HonamiDocLanguage CurrentLanguage
        {
            get => (HonamiDocLanguage)EditorPrefs.GetInt(PrefKey, (int)HonamiDocLanguage.English);
            set => EditorPrefs.SetInt(PrefKey, (int)value);
        }

        public static string Get(string en, string ua)
        {
            return CurrentLanguage == HonamiDocLanguage.Ukrainian ? ua : en;
        }
    }
}
