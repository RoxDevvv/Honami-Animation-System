using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HonamiAnimationSystem.Editor
{
    internal static class HonamiPackageInfo
    {
        private const string PackageName = "com.loyalstudio.honami-animation-system";

        private static string _version;
        private static string _repositoryUrl;
        private static string _displayName;
        private static bool _loaded;

        public static string Version { get { EnsureLoaded(); return _version; } }
        public static string ShortVersion { get { EnsureLoaded(); return "v" + SemVerBase(_version); } }
        public static string RepositoryUrl { get { EnsureLoaded(); return _repositoryUrl; } }
        public static string DisplayName { get { EnsureLoaded(); return _displayName; } }

        private static void EnsureLoaded()
        {
            if (!_loaded) Load();
        }

        private static void Load()
        {
            _loaded = true;
            _version = "0.0.0";
            _repositoryUrl = string.Empty;
            _displayName = "Honami";

            var guids = AssetDatabase.FindAssets("package", new[] { "Assets/HonamiAnimationSystem" });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith("package.json", StringComparison.OrdinalIgnoreCase)) continue;

                try
                {
                    var json = File.ReadAllText(path);
                    _version = ExtractString(json, "version") ?? _version;
                    _repositoryUrl = ExtractNestedString(json, "author", "url") ?? _repositoryUrl;
                    _displayName = ExtractString(json, "displayName") ?? _displayName;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[HonamiPackageInfo] Failed to parse package.json: {e.Message}");
                }
                break;
            }
        }

        private static string ExtractString(string json, string key)
        {
            var search = $"\"{key}\"";
            var keyIndex = json.IndexOf(search, StringComparison.Ordinal);
            if (keyIndex < 0) return null;

            var colon = json.IndexOf(':', keyIndex + search.Length);
            if (colon < 0) return null;

            var open = json.IndexOf('"', colon + 1);
            if (open < 0) return null;

            var close = json.IndexOf('"', open + 1);
            if (close < 0) return null;

            return json.Substring(open + 1, close - open - 1);
        }

        private static string ExtractNestedString(string json, string objectKey, string fieldKey)
        {
            var search = $"\"{objectKey}\"";
            var objIndex = json.IndexOf(search, StringComparison.Ordinal);
            if (objIndex < 0) return null;

            var openBrace = json.IndexOf('{', objIndex + search.Length);
            if (openBrace < 0) return null;

            var closeBrace = json.IndexOf('}', openBrace);
            if (closeBrace < 0) return null;

            var nested = json.Substring(openBrace, closeBrace - openBrace + 1);
            return ExtractString(nested, fieldKey);
        }

        private static string SemVerBase(string version)
        {
            if (string.IsNullOrEmpty(version)) return version;
            var dash = version.IndexOf('-');
            return dash > 0 ? version.Substring(0, dash) : version;
        }
    }
}
