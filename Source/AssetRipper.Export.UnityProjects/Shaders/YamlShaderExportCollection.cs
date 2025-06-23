using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.SourceGenerated.Classes.ClassID_48;

namespace AssetRipper.Export.UnityProjects.Shaders;

public sealed class YamlShaderExportCollection : AssetExportCollection<IShader>
{
	public YamlShaderExportCollection(YamlShaderExporter assetExporter, IShader asset) : base(assetExporter, asset)
	{
	}

	protected override string GetExportExtension(IUnityObjectBase asset) => "asset";

	protected override bool ExportInner(IExportContainer container, string filePath, string dirPath, FileSystem fileSystem)
	{
		// This patch uses ShaderUtil.RegisterShader(), which is only available start from Unity 2018.
		if (container.ExportVersion.GreaterThanOrEquals(2018, 1, 0))
		{
			UnityPatches.ApplyPatchFromText(RegisterShaderUnityPatchText, "YamlShaderPostprocessor", dirPath, fileSystem);
		}
		// This patch uses AssetModificationProcessor, which is only available start from Unity 3.5.
		if (container.ExportVersion.GreaterThanOrEquals(3, 5, 0))
		{
			UnityPatches.ApplyPatchFromText(FileLockerUnityPatchText, "AvoidSavingYamlShaders", dirPath, fileSystem);
		}
		return base.ExportInner(container, filePath, dirPath, fileSystem);
	}

	private const string RegisterShaderUnityPatchText = """
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
			public class YamlShaderPostprocessor : AssetPostprocessor
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
		""";

	private const string FileLockerUnityPatchText = """
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
		""";
}
