namespace AssetRipper.Export.UnityProjects.Project;

/// <summary>
/// Maps assembly names to Unity package names using a static dictionary and heuristic fallback.
/// </summary>
public static class AssemblyToPackageMapper
{
	/// <summary>
	/// Known assembly-to-package mappings extracted from .asmdef files in Unity package tarballs.
	/// Covers all official com.unity.* packages as of 2026.
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

		// com.unity.inputsystem
		["Unity.InputSystem"] = "com.unity.inputsystem",
		["Unity.InputSystem.Editor"] = "com.unity.inputsystem",
		["Unity.InputSystem.ForUI"] = "com.unity.inputsystem",
		["Unity.InputSystem.TestFramework"] = "com.unity.inputsystem",

		// com.unity.cinemachine
		["Unity.Cinemachine"] = "com.unity.cinemachine",
		["Unity.Cinemachine.Editor"] = "com.unity.cinemachine",
		["Cinemachine"] = "com.unity.cinemachine",
		["com.unity.cinemachine.editor"] = "com.unity.cinemachine",

		// com.unity.ai.navigation
		["Unity.AI.Navigation"] = "com.unity.ai.navigation",
		["Unity.AI.Navigation.Editor"] = "com.unity.ai.navigation",
		["Unity.AI.Navigation.Editor.ConversionSystem"] = "com.unity.ai.navigation",
		["Unity.AI.Navigation.Updater"] = "com.unity.ai.navigation",

		// com.unity.visualscripting
		["Unity.VisualScripting.Core"] = "com.unity.visualscripting",
		["Unity.VisualScripting.Core.Editor"] = "com.unity.visualscripting",
		["Unity.VisualScripting.Flow"] = "com.unity.visualscripting",
		["Unity.VisualScripting.Flow.Editor"] = "com.unity.visualscripting",
		["Unity.VisualScripting.State"] = "com.unity.visualscripting",
		["Unity.VisualScripting.State.Editor"] = "com.unity.visualscripting",
		["Unity.VisualScripting.SettingsProvider.Editor"] = "com.unity.visualscripting",
		["Unity.VisualScripting.Shared.Editor"] = "com.unity.visualscripting",
		["Unity.VisualScripting.Antlr3.Runtime"] = "com.unity.visualscripting",
		["Unity.VisualScripting.IonicZip"] = "com.unity.visualscripting",
		["Unity.VisualScripting.TextureAssets"] = "com.unity.visualscripting",
		["Unity.VisualScripting.YamlDotNet"] = "com.unity.visualscripting",
		["Unity.VisualScripting.sqlite3"] = "com.unity.visualscripting",

		// com.unity.analytics
		["Analytics"] = "com.unity.analytics",
		["Unity.Analytics.Editor"] = "com.unity.analytics",
		["Unity.Analytics.StandardEvents"] = "com.unity.analytics",
		["Unity.Analytics.Tracker"] = "com.unity.analytics",
		["Unity.Analytics.DataPrivacy"] = "com.unity.analytics",

		// com.unity.services.analytics
		["Unity.Services.Analytics"] = "com.unity.services.analytics",
		["Unity.Services.Analytics.Editor"] = "com.unity.services.analytics",

		// com.unity.services.core
		["Unity.Services.Core"] = "com.unity.services.core",
		["Unity.Services.Core.Analytics"] = "com.unity.services.core",
		["Unity.Services.Core.Components"] = "com.unity.services.core",
		["Unity.Services.Core.Configuration"] = "com.unity.services.core",
		["Unity.Services.Core.Configuration.Editor"] = "com.unity.services.core",
		["Unity.Services.Core.Device"] = "com.unity.services.core",
		["Unity.Services.Core.Editor"] = "com.unity.services.core",
		["Unity.Services.Core.Environments"] = "com.unity.services.core",
		["Unity.Services.Core.Environments.Editor"] = "com.unity.services.core",
		["Unity.Services.Core.Environments.Internal"] = "com.unity.services.core",
		["Unity.Services.Core.Internal"] = "com.unity.services.core",
		["Unity.Services.Core.Networking"] = "com.unity.services.core",
		["Unity.Services.Core.Registration"] = "com.unity.services.core",
		["Unity.Services.Core.Scheduler"] = "com.unity.services.core",
		["Unity.Services.Core.Telemetry"] = "com.unity.services.core",
		["Unity.Services.Core.Threading"] = "com.unity.services.core",

		// com.unity.multiplayer.center
		["Unity.Multiplayer.Center.Editor"] = "com.unity.multiplayer.center",

		// com.unity.ide.rider
		["Unity.Rider.Editor"] = "com.unity.ide.rider",
		["JetBrains.Rider.PathLocator"] = "com.unity.ide.rider",

		// com.unity.ide.visualstudio
		["Unity.VisualStudio.Editor"] = "com.unity.ide.visualstudio",

		// com.unity.ide.vscode
		["Unity.VSCode.Editor"] = "com.unity.ide.vscode",

		// com.unity.collab-proxy
		["Unity.CollabProxy.Editor"] = "com.unity.collab-proxy",
		["Unity.PlasticSCM.Editor"] = "com.unity.collab-proxy",
		["Unity.PlasticSCM.Editor.Entities"] = "com.unity.collab-proxy",
		["Unity.Plastic.Antlr3.Runtime"] = "com.unity.collab-proxy",
		["Unity.Plastic.Newtonsoft.Json"] = "com.unity.collab-proxy",
		["log4netPlastic"] = "com.unity.collab-proxy",
		["lz4x64Plastic"] = "com.unity.collab-proxy",
		["unityplastic"] = "com.unity.collab-proxy",
		["zlib64Plastic"] = "com.unity.collab-proxy",

		// com.unity.ads
		["UnityEngine.Advertisements"] = "com.unity.ads",
		["UnityEngine.Advertisements.Editor"] = "com.unity.ads",
		["UnityEngine.Advertisements.DevX.Editor"] = "com.unity.ads",

		// com.unity.test-framework
		["UnityEditor.TestRunner"] = "com.unity.test-framework",
		["UnityEngine.TestRunner"] = "com.unity.test-framework",

		// com.unity.2d.sprite
		["Unity.2D.Sprite.Editor"] = "com.unity.2d.sprite",

		// com.unity.2d.tilemap
		["Unity.2D.Tilemap.Editor"] = "com.unity.2d.tilemap",

		// com.unity.2d.animation
		["Unity.2D.Animation.Runtime"] = "com.unity.2d.animation",
		["Unity.2D.Animation.Editor"] = "com.unity.2d.animation",
		["Unity.2D.IK.Runtime"] = "com.unity.2d.animation",
		["Unity.2D.IK.Editor"] = "com.unity.2d.animation",

		// com.unity.2d.pixel-perfect
		["Unity.2D.PixelPerfect"] = "com.unity.2d.pixel-perfect",
		["Unity.2D.PixelPerfect.Editor"] = "com.unity.2d.pixel-perfect",

		// com.unity.2d.psdimporter
		["Unity.2D.Psdimporter.Editor"] = "com.unity.2d.psdimporter",
		["PsdPlugin"] = "com.unity.2d.psdimporter",

		// com.unity.nuget.newtonsoft-json (pre-compiled DLL, no .asmdef)
		["Newtonsoft.Json"] = "com.unity.nuget.newtonsoft-json",

		// com.unity.ext.nunit (pre-compiled DLL, no .asmdef)
		["nunit.framework"] = "com.unity.ext.nunit",

		// com.unity.probuilder
		["Unity.ProBuilder"] = "com.unity.probuilder",
		["Unity.ProBuilder.Editor"] = "com.unity.probuilder",
		["Unity.ProBuilder.AddOns.Editor"] = "com.unity.probuilder",
		["Unity.ProBuilder.AssetIdRemapUtility"] = "com.unity.probuilder",
		["Unity.ProBuilder.Csg"] = "com.unity.probuilder",
		["Unity.ProBuilder.KdTree"] = "com.unity.probuilder",
		["Unity.ProBuilder.Poly2Tri"] = "com.unity.probuilder",
		["Unity.ProBuilder.Stl"] = "com.unity.probuilder",

		// com.unity.progrids
		["Unity.ProGrids"] = "com.unity.progrids",
		["Unity.ProGrids.Editor"] = "com.unity.progrids",

		// com.unity.polybrush
		["Unity.Polybrush"] = "com.unity.polybrush",
		["Unity.Polybrush.Editor"] = "com.unity.polybrush",

		// com.unity.addressables
		["Unity.Addressables"] = "com.unity.addressables",
		["Unity.Addressables.Editor"] = "com.unity.addressables",
		["Unity.ResourceManager"] = "com.unity.addressables",

		// com.unity.localization
		["Unity.Localization"] = "com.unity.localization",
		["Unity.Localization.Editor"] = "com.unity.localization",

		// com.unity.splines
		["Unity.Splines"] = "com.unity.splines",
		["Unity.Splines.Editor"] = "com.unity.splines",

		// com.unity.render-pipelines.universal (URP)
		["Unity.RenderPipelines.Universal.Runtime"] = "com.unity.render-pipelines.universal",
		["Unity.RenderPipelines.Universal.Editor"] = "com.unity.render-pipelines.universal",
		["Unity.RenderPipelines.Universal.Shaders"] = "com.unity.render-pipelines.universal",
		["Unity.RenderPipeline.Universal.ShaderLibrary"] = "com.unity.render-pipelines.universal",

		// com.unity.render-pipelines.high-definition (HDRP)
		["Unity.RenderPipelines.HighDefinition.Runtime"] = "com.unity.render-pipelines.high-definition",
		["Unity.RenderPipelines.HighDefinition.Editor"] = "com.unity.render-pipelines.high-definition",

		// com.unity.render-pipelines.core
		["Unity.RenderPipelines.Core.Runtime"] = "com.unity.render-pipelines.core",
		["Unity.RenderPipelines.Core.Editor"] = "com.unity.render-pipelines.core",
		["Unity.RenderPipelines.Core.ShaderLibrary"] = "com.unity.render-pipelines.core",

		// com.unity.shadergraph
		["Unity.ShaderGraph.Editor"] = "com.unity.shadergraph",
		["Unity.ShaderGraph.Utilities"] = "com.unity.shadergraph",
		["Unity.RenderPipelines.ShaderGraph.ShaderGraphLibrary"] = "com.unity.shadergraph",

		// com.unity.postprocessing
		["Unity.Postprocessing.Runtime"] = "com.unity.postprocessing",
		["Unity.Postprocessing.Editor"] = "com.unity.postprocessing",

		// com.unity.burst
		["Unity.Burst"] = "com.unity.burst",
		["Unity.Burst.Editor"] = "com.unity.burst",
		["Unity.Burst.CodeGen"] = "com.unity.burst",

		// com.unity.collections
		["Unity.Collections"] = "com.unity.collections",
		["Unity.Collections.Editor"] = "com.unity.collections",
		["Unity.Collections.CodeGen"] = "com.unity.collections",
		["Unity.Collections.BurstCompatibilityGen"] = "com.unity.collections",
		["Unity.Collections.LowLevel.ILSupport"] = "com.unity.collections",
		["Unity.Collections.LowLevel.ILSupport.CodeGen"] = "com.unity.collections",

		// com.unity.mathematics
		["Unity.Mathematics"] = "com.unity.mathematics",
		["Unity.Mathematics.Editor"] = "com.unity.mathematics",

		// com.unity.entities
		["Unity.Entities"] = "com.unity.entities",
		["Unity.Entities.Editor"] = "com.unity.entities",
		["Unity.Entities.Build"] = "com.unity.entities",
		["Unity.Entities.CodeGen"] = "com.unity.entities",
		["Unity.Entities.Hybrid"] = "com.unity.entities",
		["Unity.Entities.Hybrid.HybridComponents"] = "com.unity.entities",
		["Unity.Entities.Editor.Properties"] = "com.unity.entities",
		["Unity.Entities.UI"] = "com.unity.entities",
		["Unity.Entities.UI.Editor"] = "com.unity.entities",
		["Unity.Core.Editor"] = "com.unity.entities",
		["Unity.Deformations"] = "com.unity.entities",
		["Unity.Mathematics.Extensions"] = "com.unity.entities",
		["Unity.Mathematics.Extensions.Hybrid"] = "com.unity.entities",
		["Unity.Scenes"] = "com.unity.entities",
		["Unity.Scenes.Editor"] = "com.unity.entities",
		["Unity.Transforms"] = "com.unity.entities",
		["Unity.Transforms.Hybrid"] = "com.unity.entities",

		// com.unity.netcode.gameobjects
		["Unity.Netcode.Runtime"] = "com.unity.netcode.gameobjects",
		["Unity.Netcode.Editor"] = "com.unity.netcode.gameobjects",
		["Unity.Netcode.Editor.CodeGen"] = "com.unity.netcode.gameobjects",
		["Unity.Netcode.PackageChecker.Editor"] = "com.unity.netcode.gameobjects",

		// com.unity.transport
		["Unity.Networking.Transport"] = "com.unity.transport",
		["Unity.Networking.Editor"] = "com.unity.transport",

		// com.unity.multiplayer.tools
		["Unity.Multiplayer.Tools.Adapters"] = "com.unity.multiplayer.tools",
		["Unity.Multiplayer.Tools.Common"] = "com.unity.multiplayer.tools",
		["Unity.Multiplayer.Tools.DependencyInjection"] = "com.unity.multiplayer.tools",
		["Unity.Multiplayer.Tools.DependencyInjection.UIElements"] = "com.unity.multiplayer.tools",
		["Unity.Multiplayer.Tools.Editor"] = "com.unity.multiplayer.tools",
		["Unity.Multiplayer.Tools.Initialization"] = "com.unity.multiplayer.tools",
		["Unity.Multiplayer.Tools.MetricEvents"] = "com.unity.multiplayer.tools",
		["Unity.Multiplayer.Tools.MetricTypes"] = "com.unity.multiplayer.tools",
		["Unity.Multiplayer.Tools.NetStats"] = "com.unity.multiplayer.tools",
		["Unity.Multiplayer.Tools.NetStats.CodeGen"] = "com.unity.multiplayer.tools",
		["Unity.Multiplayer.Tools.NetStatsMonitor.Component"] = "com.unity.multiplayer.tools",
		["Unity.Multiplayer.Tools.NetStatsMonitor.Configuration"] = "com.unity.multiplayer.tools",
		["Unity.Multiplayer.Tools.NetStatsMonitor.Editor"] = "com.unity.multiplayer.tools",
		["Unity.Multiplayer.Tools.NetStatsMonitor.Implementation"] = "com.unity.multiplayer.tools",
		["Unity.Multiplayer.Tools.NetStatsReporting"] = "com.unity.multiplayer.tools",
		["Unity.Multiplayer.Tools.NetVis.Configuration"] = "com.unity.multiplayer.tools",
		["Unity.Multiplayer.Tools.NetVis.Editor.UI"] = "com.unity.multiplayer.tools",
		["Unity.Multiplayer.Tools.NetVis.Editor.Visualization"] = "com.unity.multiplayer.tools",
		["Unity.Multiplayer.Tools.NetworkProfiler.Editor"] = "com.unity.multiplayer.tools",
		["Unity.Multiplayer.Tools.NetworkProfiler.Runtime"] = "com.unity.multiplayer.tools",
		["Unity.Multiplayer.Tools.NetworkSimulator.Editor"] = "com.unity.multiplayer.tools",
		["Unity.Multiplayer.Tools.NetworkSimulator.Runtime"] = "com.unity.multiplayer.tools",
		["Unity.Multiplayer.Tools.NetworkSolutionInterface"] = "com.unity.multiplayer.tools",
		["Unity.Multiplayer.Tools.Adapters.Ngo1"] = "com.unity.multiplayer.tools",
		["Unity.Multiplayer.Tools.Adapters.Ngo1WithUtp2"] = "com.unity.multiplayer.tools",
		["Unity.Multiplayer.Tools.Adapters.Utp2"] = "com.unity.multiplayer.tools",

		// com.unity.recorder
		["Unity.Recorder"] = "com.unity.recorder",
		["Unity.Recorder.Base"] = "com.unity.recorder",
		["Unity.Recorder.Editor"] = "com.unity.recorder",
		["FFmpeg.Encoding.Editor"] = "com.unity.recorder",

		// com.unity.xr.management
		["Unity.XR.Management"] = "com.unity.xr.management",
		["Unity.XR.Management.Editor"] = "com.unity.xr.management",
		["Unity.XR.TestSupport"] = "com.unity.xr.management",

		// com.unity.xr.openxr
		["Unity.XR.OpenXR"] = "com.unity.xr.openxr",
		["Unity.XR.OpenXR.Editor"] = "com.unity.xr.openxr",
		["Unity.XR.OpenXR.Features.ConformanceAutomation"] = "com.unity.xr.openxr",
		["Unity.XR.OpenXR.Features.MetaQuestSupport"] = "com.unity.xr.openxr",
		["Unity.XR.OpenXR.Features.MetaQuestSupport.Editor"] = "com.unity.xr.openxr",
		["Unity.XR.OpenXR.Features.MockRuntime"] = "com.unity.xr.openxr",
		["Unity.XR.OpenXR.Features.OculusQuestSupport"] = "com.unity.xr.openxr",
		["Unity.XR.OpenXR.Features.OculusQuestSupport.Editor"] = "com.unity.xr.openxr",
		["Unity.XR.OpenXR.Features.RuntimeDebugger"] = "com.unity.xr.openxr",
		["Unity.XR.OpenXR.Features.RuntimeDebugger.Editor"] = "com.unity.xr.openxr",
		["Unity.XR.OpenXR.TestTooling"] = "com.unity.xr.openxr",

		// com.unity.xr.core-utils
		["Unity.XR.CoreUtils"] = "com.unity.xr.core-utils",
		["Unity.XR.CoreUtils.Editor"] = "com.unity.xr.core-utils",

		// com.unity.xr.arfoundation (includes ARSubsystems and Simulation)
		["Unity.XR.ARFoundation"] = "com.unity.xr.arfoundation",
		["Unity.XR.ARFoundation.Editor"] = "com.unity.xr.arfoundation",
		["Unity.XR.ARFoundation.InternalUtils"] = "com.unity.xr.arfoundation",
		["Unity.XR.ARFoundation.InternalUtils.Editor"] = "com.unity.xr.arfoundation",
		["Unity.XR.ARFoundation.VisualScripting"] = "com.unity.xr.arfoundation",
		["Unity.XR.ARFoundation.VisualScripting.Editor"] = "com.unity.xr.arfoundation",
		["Unity.XR.ARAnalytics.Editor"] = "com.unity.xr.arfoundation",
		["Unity.XR.Simulation"] = "com.unity.xr.arfoundation",
		["Unity.XR.Simulation.Editor"] = "com.unity.xr.arfoundation",

		// com.unity.xr.arsubsystems (also bundled in arfoundation)
		["Unity.XR.ARSubsystems"] = "com.unity.xr.arsubsystems",
		["Unity.XR.ARSubsystems.Editor"] = "com.unity.xr.arsubsystems",

		// com.unity.xr.interaction.toolkit
		["Unity.XR.Interaction.Toolkit"] = "com.unity.xr.interaction.toolkit",
		["Unity.XR.Interaction.Toolkit.Editor"] = "com.unity.xr.interaction.toolkit",
		["Unity.XR.Interaction.Toolkit.Samples.SpatialKeyboard"] = "com.unity.xr.interaction.toolkit",
		["Unity.XR.Interaction.Toolkit.Analytics.Editor"] = "com.unity.xr.interaction.toolkit",
		["Unity.XR.Interaction.Toolkit.Analytics.Hooks.Editor"] = "com.unity.xr.interaction.toolkit",

		// com.unity.xr.legacyinputhelpers
		["UnityEngine.SpatialTracking"] = "com.unity.xr.legacyinputhelpers",
		["UnityEditor.SpatialTracking"] = "com.unity.xr.legacyinputhelpers",
		["UnityEngine.XR.LegacyInputHelpers"] = "com.unity.xr.legacyinputhelpers",
		["UnityEditor.XR.LegacyInputHelpers"] = "com.unity.xr.legacyinputhelpers",
		["Unity.XR.LegacyInputHelpers"] = "com.unity.xr.legacyinputhelpers",

		// com.unity.animation.rigging
		["Unity.Animation.Rigging"] = "com.unity.animation.rigging",
		["Unity.Animation.Rigging.Editor"] = "com.unity.animation.rigging",

		// com.unity.formats.fbx
		["Unity.Formats.Fbx.Runtime"] = "com.unity.formats.fbx",
		["Unity.Formats.Fbx.Editor"] = "com.unity.formats.fbx",

		// com.unity.terrain-tools
		["Unity.TerrainTools"] = "com.unity.terrain-tools",
		["Unity.TerrainTools.Editor"] = "com.unity.terrain-tools",

		// com.unity.purchasing
		["Unity.Purchasing"] = "com.unity.purchasing",
		["Unity.Purchasing.Apple"] = "com.unity.purchasing",
		["Unity.Purchasing.AppleMacos"] = "com.unity.purchasing",
		["Unity.Purchasing.AppleMacosStub"] = "com.unity.purchasing",
		["Unity.Purchasing.AppleStub"] = "com.unity.purchasing",
		["Unity.Purchasing.Codeless"] = "com.unity.purchasing",
		["Unity.Purchasing.Security"] = "com.unity.purchasing",
		["Unity.Purchasing.SecurityCore"] = "com.unity.purchasing",
		["Unity.Purchasing.SecurityStub"] = "com.unity.purchasing",
		["Unity.Purchasing.Stores"] = "com.unity.purchasing",
		["Unity.Purchasing.TransactionVerifier"] = "com.unity.purchasing",
		["Unity.Purchasing.Utilities"] = "com.unity.purchasing",
		["UnityEditor.Purchasing"] = "com.unity.purchasing",
		["IAPResolver"] = "com.unity.purchasing",

		// com.unity.mobile.notifications
		["Unity.Notifications"] = "com.unity.mobile.notifications",
		["Unity.Notifications.Android"] = "com.unity.mobile.notifications",
		["Unity.Notifications.iOS"] = "com.unity.mobile.notifications",
		["Unity.Notifications.Unified"] = "com.unity.mobile.notifications",

		// com.unity.mobile.android-logcat
		["Unity.Mobile.AndroidLogcat.Editor"] = "com.unity.mobile.android-logcat",

		// com.unity.adaptiveperformance
		["Unity.AdaptivePerformance"] = "com.unity.adaptiveperformance",
		["Unity.AdaptivePerformance.Editor"] = "com.unity.adaptiveperformance",
		["Unity.AdaptivePerformance.Profiler.Editor"] = "com.unity.adaptiveperformance",
		["Unity.AdaptivePerformance.Simulator.Editor"] = "com.unity.adaptiveperformance",
		["Unity.AdaptivePerformance.Simulator.Extension"] = "com.unity.adaptiveperformance",
		["Unity.AdaptivePerformance.UI.Editor"] = "com.unity.adaptiveperformance",

		// com.unity.device-simulator
		["Unity.DeviceSimulator.Editor"] = "com.unity.device-simulator",

		// com.unity.sequences
		["Unity.Sequences"] = "com.unity.sequences",
		["Unity.Sequences.Editor"] = "com.unity.sequences",

		// com.unity.film-internal-utilities
		["Unity.FilmInternalUtilities"] = "com.unity.film-internal-utilities",
		["Unity.FilmInternalUtilities.Editor"] = "com.unity.film-internal-utilities",

		// com.unity.live-capture
		["Unity.LiveCapture"] = "com.unity.live-capture",
		["Unity.LiveCapture.Editor"] = "com.unity.live-capture",
		["Unity.LiveCapture.ARKitFaceCapture"] = "com.unity.live-capture",
		["Unity.LiveCapture.ARKitFaceCapture.Editor"] = "com.unity.live-capture",
		["Unity.LiveCapture.Cameras"] = "com.unity.live-capture",
		["Unity.LiveCapture.Cameras.Editor"] = "com.unity.live-capture",
		["Unity.LiveCapture.CompanionApp"] = "com.unity.live-capture",
		["Unity.LiveCapture.CompanionApp.Editor"] = "com.unity.live-capture",
		["Unity.LiveCapture.Ltc"] = "com.unity.live-capture",
		["Unity.LiveCapture.Ltc.Editor"] = "com.unity.live-capture",
		["Unity.LiveCapture.Mocap"] = "com.unity.live-capture",
		["Unity.LiveCapture.Mocap.Editor"] = "com.unity.live-capture",
		["Unity.LiveCapture.Networking"] = "com.unity.live-capture",
		["Unity.LiveCapture.Ntp"] = "com.unity.live-capture",
		["Unity.LiveCapture.Ntp.Editor"] = "com.unity.live-capture",
		["Unity.LiveCapture.Rendering"] = "com.unity.live-capture",
		["Unity.LiveCapture.Rendering.Editor"] = "com.unity.live-capture",
		["Unity.LiveCapture.TransformCapture"] = "com.unity.live-capture",
		["Unity.LiveCapture.TransformCapture.Editor"] = "com.unity.live-capture",
		["Unity.LiveCapture.VideoStreaming.Server"] = "com.unity.live-capture",
		["Unity.LiveCapture.VirtualCamera"] = "com.unity.live-capture",
		["Unity.LiveCapture.VirtualCamera.Editor"] = "com.unity.live-capture",
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
