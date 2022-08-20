using System;
using UnityEngine;
using UnityEditor;

namespace AssetRipperPatches.Editor
{
    /// <summary>
    /// This script is AssetRipper's patch for shaders exported as YAML assets.
    /// Such a shader can be assigned to and used by a material as a regular .shader asset,
    /// but it does not work with Shader.Find() unless we explicitly register it. 
    /// Note that this patch only works for a limited range of Unity versions
    /// since it uses ShaderUtil.RegisterShader(), which is only available in Unity 2018+.
    /// </summary>
    public class AssetRipperShaderPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var importedAsset in importedAssets)
            {
                if (!importedAsset.EndsWith(".asset", StringComparison.Ordinal)) continue;
                Shader yamlShader = AssetDatabase.LoadMainAssetAtPath(importedAsset) as Shader;
                if (yamlShader == null) continue;
                ShaderUtil.RegisterShader(yamlShader);
                Debug.Log($"Registered shader \"{yamlShader.name}\" from {importedAsset}");
            }
        }
    }
}
