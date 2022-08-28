using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetRipperPatches.Editor
{
    /// <summary>
    /// This script is AssetRipper's patch for shaders exported as YAML assets.
    /// Such a shader asset can be corrupted if Unity Editor thinks it is dirty and tries to save it.
    /// Manual repro of the issue is easy as a simple call of EditorUtility.SetDirty(someYamlShaderAsset) followed by a Save Project.
    /// Hence we use this script to prevent Unity from saving YAML shader assets.
    /// </summary>
    class AvoidSavingYamlShaders
        // AssetModificationProcessor is a new API added since Unity 3.5. However, it is not in the UnityEditor namespace until Unity 4.0.
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5 || UNITY_2017_1_OR_NEWER
        : UnityEditor.AssetModificationProcessor
#elif UNITY_3_5
        : AssetModificationProcessor
#endif
    {
        private static readonly List<string> _pathList = new List<string>();

        private static string[] OnWillSaveAssets(string[] paths)
        {
            _pathList.Clear();
            foreach (string path in paths)
            {
                if (path.EndsWith(".asset", StringComparison.Ordinal) && AssetDatabase.LoadMainAssetAtPath(path) is Shader)
                {
                    Debug.Log(string.Format("Unity's attempt to overwrite the YAML Shader asset has been blocked: {0}", path));
                }
                else
                {
                    _pathList.Add(path);
                }
            }
            return _pathList.ToArray();
        }
    }
}
