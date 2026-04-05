namespace AssetRipper.Export.UnityProjects.Project;

/// <summary>
/// Maps assembly names to Unity package names using a static dictionary and heuristic fallback.
/// </summary>
public static class AssemblyToPackageMapper
{
	/// <summary>
	/// Known assembly-to-package mappings that cannot be derived by heuristic.
	/// </summary>
	private static readonly Dictionary<string, string> KnownMappings = new(StringComparer.Ordinal)
	{
		// com.unity.ugui
		["UnityEngine.UI"] = "com.unity.ugui",
		["UnityEditor.UI"] = "com.unity.ugui",

		// com.unity.textmeshpro (separate package in Unity < 2023, merged into ugui in Unity 6+)
		["Unity.TextMeshPro"] = "com.unity.textmeshpro",
		["Unity.TextMeshPro.Editor"] = "com.unity.textmeshpro",

		// com.unity.timeline
		["Unity.Timeline"] = "com.unity.timeline",
		["Unity.Timeline.Editor"] = "com.unity.timeline",

		// com.unity.ai.navigation
		["Unity.AI.Navigation"] = "com.unity.ai.navigation",
		["Unity.AI.Navigation.Editor"] = "com.unity.ai.navigation",
		["Unity.AI.Navigation.Editor.ConversionSystem"] = "com.unity.ai.navigation",

		// com.unity.visualscripting
		["Unity.VisualScripting.Core"] = "com.unity.visualscripting",
		["Unity.VisualScripting.Flow"] = "com.unity.visualscripting",
		["Unity.VisualScripting.State"] = "com.unity.visualscripting",
		["Unity.VisualScripting.Core.Editor"] = "com.unity.visualscripting",
		["Unity.VisualScripting.Flow.Editor"] = "com.unity.visualscripting",
		["Unity.VisualScripting.SettingsProvider.Editor"] = "com.unity.visualscripting",
		["Unity.VisualScripting.Shared.Editor"] = "com.unity.visualscripting",
		["Unity.VisualScripting.Antlr3.Runtime"] = "com.unity.visualscripting",
		["Unity.VisualScripting.IonicZip"] = "com.unity.visualscripting",
		["Unity.VisualScripting.TextureAssets"] = "com.unity.visualscripting",
		["Unity.VisualScripting.YamlDotNet"] = "com.unity.visualscripting",
		["Unity.VisualScripting.sqlite3"] = "com.unity.visualscripting",

		// com.unity.analytics
		["Unity.Analytics.Editor"] = "com.unity.analytics",
		["Unity.Analytics.StandardEvents"] = "com.unity.analytics",
		["Unity.Analytics.Tracker"] = "com.unity.analytics",
		["Unity.Analytics.DataPrivacy"] = "com.unity.analytics",

		// com.unity.services.analytics
		["Unity.Services.Analytics"] = "com.unity.services.analytics",
		["Unity.Services.Analytics.Editor"] = "com.unity.services.analytics",

		// com.unity.services.core
		["Unity.Services.Core.Components"] = "com.unity.services.core",
		["Unity.Services.Core.Editor"] = "com.unity.services.core",

		// com.unity.multiplayer.center
		["Unity.Multiplayer.Center.Editor"] = "com.unity.multiplayer.center",

		// com.unity.ide.rider
		["Unity.Rider.Editor"] = "com.unity.ide.rider",
		["JetBrains.Rider.PathLocator"] = "com.unity.ide.rider",

		// com.unity.collab-proxy
		["Unity.PlasticSCM.Editor"] = "com.unity.collab-proxy",
		["Unity.Plastic.Antlr3.Runtime"] = "com.unity.collab-proxy",
		["Unity.Plastic.Newtonsoft.Json"] = "com.unity.collab-proxy",
		["log4netPlastic"] = "com.unity.collab-proxy",
		["lz4x64Plastic"] = "com.unity.collab-proxy",
		["unityplastic"] = "com.unity.collab-proxy",
		["zlib64Plastic"] = "com.unity.collab-proxy",

		// com.unity.ads
		["UnityEngine.Advertisements"] = "com.unity.ads",

		// com.unity.test-framework
		["UnityEditor.TestRunner"] = "com.unity.test-framework",
		["UnityEngine.TestRunner"] = "com.unity.test-framework",

		// com.unity.2d.sprite
		["Unity.2D.Sprite.Editor"] = "com.unity.2d.sprite",

		// com.unity.2d.tilemap
		["Unity.2D.Tilemap.Editor"] = "com.unity.2d.tilemap",

		// com.unity.nuget.newtonsoft-json
		["Newtonsoft.Json"] = "com.unity.nuget.newtonsoft-json",

		// com.unity.ext.nunit
		["nunit.framework"] = "com.unity.ext.nunit",

		// com.unity.inputsystem
		["Unity.InputSystem"] = "com.unity.inputsystem",
		["Unity.InputSystem.Editor"] = "com.unity.inputsystem",

		// com.unity.cinemachine
		["Unity.Cinemachine"] = "com.unity.cinemachine",
		["Unity.Cinemachine.Editor"] = "com.unity.cinemachine",
		["Cinemachine"] = "com.unity.cinemachine",

		// com.unity.probuilder (includes sub-assemblies KdTree, Poly2Tri, Stl)
		["Unity.ProBuilder"] = "com.unity.probuilder",
		["Unity.ProBuilder.Editor"] = "com.unity.probuilder",
		["Unity.ProBuilder.KdTree"] = "com.unity.probuilder",
		["Unity.ProBuilder.Poly2Tri"] = "com.unity.probuilder",
		["Unity.ProBuilder.Stl"] = "com.unity.probuilder",
		["Unity.ProBuilder.Csg"] = "com.unity.probuilder",

		// com.unity.progrids
		["Unity.ProGrids"] = "com.unity.progrids",
		["Unity.ProGrids.Editor"] = "com.unity.progrids",

		// com.unity.addressables
		["Unity.Addressables"] = "com.unity.addressables",
		["Unity.Addressables.Editor"] = "com.unity.addressables",
		["Unity.ResourceManager"] = "com.unity.addressables",

		// com.unity.localization
		["Unity.Localization"] = "com.unity.localization",
		["Unity.Localization.Editor"] = "com.unity.localization",

		// com.unity.render-pipelines.universal
		["Unity.RenderPipelines.Universal.Runtime"] = "com.unity.render-pipelines.universal",
		["Unity.RenderPipelines.Universal.Editor"] = "com.unity.render-pipelines.universal",
		["Unity.RenderPipelines.Universal.Shaders"] = "com.unity.render-pipelines.universal",

		// com.unity.render-pipelines.high-definition
		["Unity.RenderPipelines.HighDefinition.Runtime"] = "com.unity.render-pipelines.high-definition",
		["Unity.RenderPipelines.HighDefinition.Editor"] = "com.unity.render-pipelines.high-definition",

		// com.unity.render-pipelines.core
		["Unity.RenderPipelines.Core.Runtime"] = "com.unity.render-pipelines.core",
		["Unity.RenderPipelines.Core.Editor"] = "com.unity.render-pipelines.core",
		["Unity.RenderPipelines.ShaderGraph.ShaderGraphLibrary"] = "com.unity.shadergraph",

		// com.unity.xr.management
		["Unity.XR.Management"] = "com.unity.xr.management",
		["Unity.XR.Management.Editor"] = "com.unity.xr.management",

		// com.unity.xr.openxr
		["Unity.XR.OpenXR"] = "com.unity.xr.openxr",

		// com.unity.burst
		["Unity.Burst"] = "com.unity.burst",
		["Unity.Burst.Editor"] = "com.unity.burst",

		// com.unity.collections
		["Unity.Collections"] = "com.unity.collections",

		// com.unity.mathematics
		["Unity.Mathematics"] = "com.unity.mathematics",

		// com.unity.entities
		["Unity.Entities"] = "com.unity.entities",
		["Unity.Entities.Editor"] = "com.unity.entities",

		// com.unity.netcode.gameobjects
		["Unity.Netcode.Runtime"] = "com.unity.netcode.gameobjects",
		["Unity.Netcode.Editor"] = "com.unity.netcode.gameobjects",

		// com.unity.recorder
		["Unity.Recorder"] = "com.unity.recorder",
		["Unity.Recorder.Editor"] = "com.unity.recorder",

		// com.unity.2d.animation
		["Unity.2D.Animation.Runtime"] = "com.unity.2d.animation",
		["Unity.2D.Animation.Editor"] = "com.unity.2d.animation",

		// com.unity.postprocessing
		["Unity.Postprocessing.Runtime"] = "com.unity.postprocessing",
		["Unity.Postprocessing.Editor"] = "com.unity.postprocessing",
	};

	/// <summary>
	/// Fallback versions for packages that are embedded in Unity and may not be on the public registry.
	/// These are the versions commonly shipped with their respective Unity versions.
	/// </summary>
	private static readonly Dictionary<string, string> FallbackVersions = new(StringComparer.Ordinal)
	{
		["com.unity.ugui"] = "1.0.0",
		["com.unity.2d.sprite"] = "1.0.0",
		["com.unity.2d.tilemap"] = "1.0.0",
	};

	/// <summary>
	/// Get a fallback version for packages that may not be on the public registry.
	/// </summary>
	public static string? GetFallbackVersion(string packageName)
	{
		return FallbackVersions.GetValueOrDefault(packageName);
	}

	/// <summary>
	/// Try to map an assembly name to a Unity package name.
	/// First checks the static mapping, then falls back to heuristic pattern matching.
	/// </summary>
	/// <returns>The package name, or null if the assembly cannot be mapped.</returns>
	public static string? TryGetPackageName(string assemblyName)
	{
		if (KnownMappings.TryGetValue(assemblyName, out string? packageName))
		{
			return packageName;
		}

		return TryHeuristicMapping(assemblyName);
	}

	/// <summary>
	/// Heuristic: Try to derive a package name from the assembly name pattern.
	/// Only works for Unity packages following standard naming conventions.
	/// </summary>
	private static string? TryHeuristicMapping(string assemblyName)
	{
		// Pattern: Unity.{Name} or Unity.{Name}.Editor or Unity.{Name}.Runtime
		if (!assemblyName.StartsWith("Unity.", StringComparison.Ordinal))
		{
			return null;
		}

		// Remove "Unity." prefix
		string remainder = assemblyName["Unity.".Length..];

		// Remove common suffixes
		string[] suffixesToRemove = [".Editor", ".Runtime", ".Tests"];
		foreach (string suffix in suffixesToRemove)
		{
			if (remainder.EndsWith(suffix, StringComparison.Ordinal))
			{
				remainder = remainder[..^suffix.Length];
				break;
			}
		}

		// Skip single-segment names that are likely part of Unity engine (e.g., Unity.Collections might be fine)
		if (string.IsNullOrEmpty(remainder))
		{
			return null;
		}

		// Convert to package name: Unity.SomeThing → com.unity.something
		string candidatePackage = $"com.unity.{remainder.ToLowerInvariant()}";

		return candidatePackage;
	}
}
