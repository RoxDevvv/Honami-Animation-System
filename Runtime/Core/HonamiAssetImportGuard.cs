#if UNITY_EDITOR
using UnityEditor;

namespace HonamiAnimationSystem.Runtime.Core
{
    internal static class HonamiAssetImportGuard
    {
        // OnValidate mutations during import make NativeFormatImporter results non-deterministic
        public static bool IsImportingAssets =>
            EditorApplication.isUpdating || AssetDatabase.IsAssetImportWorkerProcess();
    }
}
#endif
