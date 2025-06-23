using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.SourceGenerated.Classes.ClassID_240;
using AssetRipper.SourceGenerated.Classes.ClassID_243;
using AssetRipper.SourceGenerated.Classes.ClassID_273;

namespace AssetRipper.Export.UnityProjects.AudioMixers;

public class AudioMixerExportCollection : AssetsExportCollection<IAudioMixer>
{
	public AudioMixerExportCollection(IAssetExporter assetExporter, IAudioMixer mixer) : base(assetExporter, mixer)
	{
		foreach (IAudioMixerGroup group in FindChildren(mixer))
		{
			AddAsset(group);
			if (group is IAudioMixerGroupController controller)
			{
				AddAssets(controller.EffectsP);
			}
		}
		AddAssets(mixer.Snapshots_C240P);
	}

	private static IEnumerable<IAudioMixerGroup> FindChildren(IAudioMixer mixer)
	{
		//IAudioMixerController.Children might give the same information and be way more efficient.
		foreach (IAudioMixerGroup group in mixer.Collection.Bundle.FetchAssetsInHierarchy().OfType<IAudioMixerGroup>())
		{
			if (group.AudioMixer_C273P == mixer)
			{
				yield return group;
			}
		}
	}

	private uint m_nextExportID;

	protected override long GenerateExportID(IUnityObjectBase asset)
	{
		long exportID = ExportIdHandler.GetMainExportID(asset, m_nextExportID);
		m_nextExportID += 2;
		return exportID;
	}

	protected override bool ExportInner(IExportContainer container, string filePath, string dirPath, FileSystem fileSystem)
	{
		UnityPatches.ApplyPatchFromText(AudioMixerPatchText, "AudioMixerPostprocessor", dirPath, fileSystem);
		return base.ExportInner(container, filePath, dirPath, fileSystem);
	}

	private const string AudioMixerPatchText = """
		using System;
		using System.Reflection;
		using UnityEditor;
		using UnityEngine;
		using Object = UnityEngine.Object;

		namespace AssetRipperPatches.Editor
		{
			/// <summary>
			/// This script is AssetRipper's patch for exported audio effects to recover effect parameter names when Unity imports each audio mixer.
			/// Unity does not serialize the parameter names in a release asset, so it is impossible to recover them by AssetRipper.
			/// Fortunately, there is an internal function <c>AudioMixerEffectController.PreallocateGUIDs</c> in UnityEditor.dll, which can help us.
			/// This function is used by Unity Editor when creating a new audio effect. It collects a list of runtime audio effects,
			/// retrieves parameter definitions for each, and updates the parameter names and GUIDs in the caller AudioMixerEffectController.
			/// Moreover, this function won't update the GUID for a parameter if it already has a non-empty GUID,
			/// which is the case in exported audio effects, perfectly matching our needs.
			/// </summary>
			public class AudioMixerPostprocessor : AssetPostprocessor
			{
				private static readonly Type AudioMixerEffectControllerType;
				private static readonly MethodInfo PreallocateGUIDsMethod;
				private static readonly MethodInfo GetAudioEffectNamesMethod;

				static AudioMixerPostprocessor()
				{
					Assembly editorAssembly = typeof(AssetPostprocessor).Assembly;
					AudioMixerEffectControllerType = editorAssembly.GetType("UnityEditor.Audio.AudioMixerEffectController", true);
					PreallocateGUIDsMethod = AudioMixerEffectControllerType.GetMethod("PreallocateGUIDs", BindingFlags.Public | BindingFlags.Instance);
					if (PreallocateGUIDsMethod == null)
					{
						Debug.LogError("AudioMixerEffectController.PreallocateGUIDs() method is missing in this version of Unity. Audio effect parameter values will be reset to default.");
					}

					Type mixerEffectDefinitionsType = editorAssembly.GetType("UnityEditor.Audio.MixerEffectDefinitions", true);
					GetAudioEffectNamesMethod = mixerEffectDefinitionsType.GetMethod("GetAudioEffectNames", BindingFlags.Public | BindingFlags.Static);
				}

				static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
				{
					if (PreallocateGUIDsMethod == null) return;

					bool printEffectNames = GetAudioEffectNamesMethod != null;

					foreach (string importedAsset in importedAssets)
					{
						if (importedAsset.EndsWith(".mixer"))
						{
							foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(importedAsset))
							{
								if (asset.GetType() == AudioMixerEffectControllerType)
								{
									if (printEffectNames)
									{
										printEffectNames = false;
										string[] effectNames = (string[])GetAudioEffectNamesMethod.Invoke(null, new object[0]);
										Debug.LogFormat("MixerEffectDefinitions.GetAudioEffectNames returns [{0}]", String.Join(", ", effectNames));
									}
									PreallocateGUIDsMethod.Invoke(asset, new object[0]);
									Debug.LogFormat("AudioMixerEffectController.PreallocateGUIDs has been called on {0}", asset);
									EditorUtility.SetDirty(asset);
								}
							}
						}
					}
				}
			}
		}
		""";
}
