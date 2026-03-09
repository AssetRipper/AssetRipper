using AssetRipper.Export.Configuration;
using AssetRipper.Processing;

namespace AssetRipper.Export.UnityProjects.Project;

public sealed class AddressablesPostExporter(RegistryPackageBridge registryPackageBridge) : IPostExporter
{
	private const string AddressablesPackageId = "com.unity.addressables";
	private const string AddressablesRoot = "AddressableAssetsData";

	public void DoPostExport(GameData gameData, FullConfiguration settings, FileSystem fileSystem)
	{
		if (!registryPackageBridge.ManifestDependencies.ContainsKey(AddressablesPackageId))
		{
			return;
		}

		string addressablesDirectory = fileSystem.Path.Join(settings.AssetsPath, AddressablesRoot);
		if (!fileSystem.Directory.Exists(addressablesDirectory))
		{
			return;
		}

		UnityPatches.ApplyPatchFromText(EditorPatchText, "AddressablesBootstrap", settings.ProjectRootPath, fileSystem);
	}

	private const string EditorPatchText = """
#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace AssetRipperPatches
{
	internal static class AddressablesBootstrap
	{
		private static readonly string[] SearchRoots = { "Assets/AddressableAssetsData" };

		static AddressablesBootstrap()
		{
			EditorApplication.delayCall += TryRepair;
		}

		[MenuItem("Tools/AssetRipper/Repair Addressables Settings")]
		private static void RepairFromMenu()
		{
			Repair(verbose: true);
		}

		private static void TryRepair()
		{
			if (EditorApplication.isCompiling || EditorApplication.isUpdating)
			{
				EditorApplication.delayCall += TryRepair;
				return;
			}

			Repair(verbose: false);
		}

		private static void Repair(bool verbose)
		{
			string[] guids = AssetDatabase.FindAssets("t:AddressableAssetSettings", SearchRoots);
			if (guids is null || guids.Length == 0)
			{
				return;
			}

			string settingsPath = guids
				.Select(AssetDatabase.GUIDToAssetPath)
				.Where(static path => !string.IsNullOrWhiteSpace(path))
				.OrderBy(static path => !string.Equals(path, AddressableAssetSettingsDefaultObject.DefaultAssetPath, StringComparison.OrdinalIgnoreCase))
				.ThenBy(static path => path, StringComparer.OrdinalIgnoreCase)
				.FirstOrDefault();

			if (string.IsNullOrWhiteSpace(settingsPath))
			{
				return;
			}

			AddressableAssetSettings settings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(settingsPath);
			if (settings == null)
			{
				return;
			}

			if (ReferenceEquals(AddressableAssetSettingsDefaultObject.Settings, settings))
			{
				return;
			}

			AddressableAssetSettingsDefaultObject.Settings = settings;
			if (verbose)
			{
				Debug.Log("AssetRipper: Addressables default settings repaired at " + settingsPath);
			}
		}
	}
}
#endif
""";
}
