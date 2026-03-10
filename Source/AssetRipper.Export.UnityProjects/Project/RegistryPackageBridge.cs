using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AssetRipper.Assets;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.IO.Files;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AssetRipper.Export.UnityProjects.Project;

public sealed class RegistryPackageBridge
{
	private const string UnityPackageCacheRelativePath = @"Unity\cache\packages\packages.unity.com";
	private static readonly Regex PackageTagRegex = new(@"(?i)(?:packages[\\/])?(?<pkg>(?:com|io)\.[a-z0-9][a-z0-9._-]+)@(?<ver>[0-9]+(?:\.[0-9]+){1,3}(?:[-+][A-Za-z0-9_.-]+)?)", RegexOptions.Compiled);
	private static readonly Regex PackageReferenceRegex = new(@"(?i)(?:PackageCache|Packages)[\\/](?<pkg>(?:com|io)\.[a-z0-9][a-z0-9._-]+)(?:@(?<ref>[A-Za-z0-9._-]+))?(?=[\\/])", RegexOptions.Compiled);
	private static readonly Regex PackageIdRegex = new(@"^(?:com|io)\.[a-z0-9][a-z0-9._-]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
	private static readonly Regex SemanticVersionRegex = new(@"(?<![0-9])(?<ver>[0-9]+(?:\.[0-9]+){1,3}(?:[-+][A-Za-z0-9_.-]+)?)(?![0-9])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
	private static readonly Regex NamespaceRegex = new(@"^\s*namespace\s+([A-Za-z_][\w\.]*)\s*(?:;|\{)", RegexOptions.Compiled | RegexOptions.Multiline);
	private static readonly Regex TypeNameRegex = new(@"^\s*(?:\[[^\]]+\]\s*)*(?:(?:public|internal|protected|private|sealed|abstract|static|partial|unsafe|new)\s+)*(?:class|struct|interface|enum)\s+([A-Za-z_][\w]*)", RegexOptions.Compiled | RegexOptions.Multiline);
	private static readonly Regex GuidRegex = new(@"^\s*guid:\s*([0-9a-fA-F]{32})\s*$", RegexOptions.Compiled | RegexOptions.Multiline);
	private static readonly Regex ShaderNameRegex = new("Shader\\s+\"([^\"]+)\"", RegexOptions.Compiled | RegexOptions.Multiline);
	private static readonly long MonoScriptFileID = ExportIdHandler.GetMainExportID((int)ClassIDType.MonoScript);
	private static readonly long ShaderFileID = ExportIdHandler.GetMainExportID((int)ClassIDType.Shader);
	private static readonly Dictionary<string, FamilyHint> FamilyHints = new(StringComparer.Ordinal)
	{
		["Cinemachine.dll"] = new("com.unity.cinemachine", "Unity Cinemachine", "com.unity.cinemachine", ["cinemachine"]),
		["Unity.Addressables.dll"] = new("com.unity.addressables", "Unity Addressables", "com.unity.addressables", ["addressables"]),
		["Unity.ResourceManager.dll"] = new("com.unity.addressables", "Unity Addressables", "com.unity.addressables", ["addressables", "resourcemanager"]),
		["Unity.AI.Navigation.dll"] = new("com.unity.ai.navigation", "Unity AI Navigation", "com.unity.ai.navigation", ["navigation"]),
		["Unity.Animation.Rigging.dll"] = new("com.unity.animation.rigging", "Unity Animation Rigging", "com.unity.animation.rigging", ["rigging"]),
		["Unity.Burst.dll"] = new("com.unity.burst", "Unity Burst", "com.unity.burst", ["burst"]),
		["Unity.Collections.dll"] = new("com.unity.collections", "Unity Collections", "com.unity.collections", ["collections"]),
		["Autodesk.Fbx.dll"] = new("com.autodesk.fbx", "Autodesk FBX", "com.autodesk.fbx", ["fbx", "autodesk"]),
		["Unity.Formats.Fbx.Runtime.dll"] = new("com.autodesk.fbx", "Autodesk FBX", "com.autodesk.fbx", ["fbx", "autodesk"]),
		["Unity.InputSystem.dll"] = new("com.unity.inputsystem", "Unity Input System", "com.unity.inputsystem", ["inputsystem"]),
		["Unity.Mathematics.dll"] = new("com.unity.mathematics", "Unity Mathematics", "com.unity.mathematics", ["mathematics"]),
		["Unity.ProBuilder.dll"] = new("com.unity.probuilder", "Unity ProBuilder", "com.unity.probuilder", ["probuilder"]),
		["Unity.Profiling.Core.dll"] = new("com.unity.profiling.core", "Unity Profiling Core", "com.unity.profiling.core", ["profiling"]),
		["Unity.RenderPipelines.Core.Runtime.dll"] = new("com.unity.render-pipelines.core", "Unity RP Core", "com.unity.render-pipelines.core", ["render-pipelines", "srp"]),
		["Unity.RenderPipelines.Core.ShaderLibrary.dll"] = new("com.unity.render-pipelines.core", "Unity RP Core", "com.unity.render-pipelines.core", ["render-pipelines", "srp"]),
		["Unity.RenderPipelines.Universal.Runtime.dll"] = new("com.unity.render-pipelines.universal", "Unity URP", "com.unity.render-pipelines.universal", ["urp", "universal"]),
		["Unity.RenderPipeline.Universal.ShaderLibrary.dll"] = new("com.unity.render-pipelines.universal", "Unity URP", "com.unity.render-pipelines.universal", ["urp", "universal"]),
		["Unity.RenderPipelines.Universal.Shaders.dll"] = new("com.unity.render-pipelines.universal", "Unity URP", "com.unity.render-pipelines.universal", ["urp", "universal"]),
		["Unity.RenderPipelines.ShaderGraph.ShaderGraphLibrary.dll"] = new("com.unity.shadergraph", "Unity Shader Graph", "com.unity.shadergraph", ["shadergraph"]),
		["Unity.ScriptableBuildPipeline.dll"] = new("com.unity.scriptablebuildpipeline", "Unity Scriptable Build Pipeline", "com.unity.scriptablebuildpipeline", ["scriptablebuildpipeline"]),
		["Unity.Splines.dll"] = new("com.unity.splines", "Unity Splines", "com.unity.splines", ["splines"]),
		["Unity.TextMeshPro.dll"] = new("com.unity.textmeshpro", "TextMeshPro", "com.unity.textmeshpro", ["textmeshpro"]),
		["Unity.Timeline.dll"] = new("com.unity.timeline", "Unity Timeline", "com.unity.timeline", ["timeline"]),
		["Unity.XR.CoreUtils.dll"] = new("com.unity.xr.core-utils", "XR Core Utils", "com.unity.xr.core-utils", ["xr", "coreutils"]),
		["Unity.XR.Interaction.Toolkit.dll"] = new("com.unity.xr.interaction.toolkit", "XR Interaction Toolkit", "com.unity.xr.interaction.toolkit", ["xr", "interaction toolkit"]),
		["Unity.XR.Management.dll"] = new("com.unity.xr.management", "XR Management", "com.unity.xr.management", ["xr", "management"]),
		["Unity.XR.Oculus.dll"] = new("com.unity.xr.oculus", "Unity XR Oculus", "com.unity.xr.oculus", ["xr", "oculus"]),
		["UnityEngine.SpatialTracking.dll"] = new("com.unity.xr.legacyinputhelpers", "XR Legacy Input Helpers", "com.unity.xr.legacyinputhelpers", ["legacyinputhelpers", "spatialtracking"]),
		["UnityEngine.XR.LegacyInputHelpers.dll"] = new("com.unity.xr.legacyinputhelpers", "XR Legacy Input Helpers", "com.unity.xr.legacyinputhelpers", ["legacyinputhelpers"]),
		["UnityEngine.UI.dll"] = new("com.unity.ugui", "Unity UI", "com.unity.ugui", ["ugui", "ui"]),
		["Fusion.Common.dll"] = new("com.photonengine.fusion", "Photon Fusion", null, ["fusion"]),
		["Fusion.Log.dll"] = new("com.photonengine.fusion", "Photon Fusion", null, ["fusion"]),
		["Fusion.Realtime.dll"] = new("com.photonengine.fusion", "Photon Fusion", null, ["fusion"]),
		["Fusion.Runtime.dll"] = new("com.photonengine.fusion", "Photon Fusion", null, ["fusion"]),
		["Fusion.Sockets.dll"] = new("com.photonengine.fusion", "Photon Fusion", null, ["fusion"]),
		["Fusion.Unity.dll"] = new("com.photonengine.fusion", "Photon Fusion", null, ["fusion"]),
		["PhotonRealtime.dll"] = new("com.exitgames.photonrealtime", "Photon Realtime", null, ["photon", "realtime", "version"]),
		["PhotonUnityNetworking.dll"] = new("com.exitgames.photonpun", "Photon PUN 2", null, ["photon.pun", "punversion", "pun"]),
		["PhotonVoice.dll"] = new("com.exitgames.photonvoice", "Photon Voice", null, ["photon", "voice", "version"]),
		["PhotonVoice.API.dll"] = new("com.exitgames.photonvoice", "Photon Voice API", null, ["photon", "voice"]),
		["PlayFab.dll"] = new("com.playfab.xplatcppsdk", "PlayFab SDK", null, ["playfab", "version", "sdk"]),
		["PlayFabAllModels.dll"] = new("com.playfab.xplatcppsdk", "PlayFab SDK Models", null, ["playfab"]),
		["PlayFabErrors.dll"] = new("com.playfab.xplatcppsdk", "PlayFab SDK Errors", null, ["playfab"]),
		["VRRig.dll"] = new("com.anotheraxiom.vrrig", "VRRig", null, ["vrrig", "version"]),
		["GorillaLocomotion.dll"] = new("com.anotheraxiom.gorillalocomotion", "Gorilla Locomotion", null, ["gorillalocomotion", "version"]),
		["modio.UnityPlugin.dll"] = new("com.modio.modio-unity", "mod.io Unity Plugin", null, ["mod.io", "modio"]),
		["Oculus.VR.dll"] = new("com.meta.xr.sdk.all", "Meta XR SDK All", null, ["oculus", "ovr", "meta", "version"]),
		["Oculus.Interaction.dll"] = new("com.meta.xr.sdk.interaction", "Meta XR Interaction", null, ["oculus", "interaction", "meta"]),
		["VoiceSDK.Runtime.dll"] = new("com.meta.xr.sdk.voice", "Meta Voice SDK", null, ["voicesdk", "voice sdk", "meta"]),
		["SteamVR.dll"] = new("com.valvesoftware.unity.openvr", "SteamVR OpenVR Plugin", null, ["steamvr", "valve", "openvr", "version"]),
		["SteamVR_Actions.dll"] = new("com.valvesoftware.unity.openvr", "SteamVR Actions", null, ["steamvr", "actions", "valve"]),
		["Valve.Newtonsoft.Json.dll"] = new("com.valvesoftware.unity.openvr", "SteamVR Newtonsoft", null, ["steamvr", "valve"]),
		["Newtonsoft.Json.dll"] = new("com.unity.nuget.newtonsoft-json", "Newtonsoft.Json", null, ["newtonsoft", "json", "version"]),
	};
	private static readonly HashSet<string> ManifestSources = new(StringComparer.Ordinal)
	{
		"explicit_tag",
		"assembly_metadata",
		"context_strings",
	};

	private readonly Dictionary<string, Candidate> manifestCandidates = new(StringComparer.Ordinal);
	private readonly Dictionary<string, Candidate> familyCandidates = new(StringComparer.Ordinal);
	private readonly Dictionary<string, FamilyHint> familyHintsByKey = new(StringComparer.Ordinal);
	private readonly Dictionary<string, PackageDirectoryResolution> packageDirectories = new(StringComparer.Ordinal);
	private readonly Lazy<Dictionary<ScriptIdentity, MetaPtr>> scriptPointerIndex;
	private readonly Lazy<Dictionary<string, MetaPtr>> shaderPointerIndex;

	private readonly HashSet<string> allAssemblies = new(StringComparer.Ordinal);
	private readonly Dictionary<string, string> assemblyVersions = new(StringComparer.Ordinal);
	public RegistryPackageBridge(IAssemblyManager assemblyManager, UnityVersion version)
	{
		AssemblyManager = assemblyManager;
		Version = version;
		PackageCacheRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), UnityPackageCacheRelativePath);
		scriptPointerIndex = new Lazy<Dictionary<ScriptIdentity, MetaPtr>>(BuildScriptPointerIndex, LazyThreadSafetyMode.ExecutionAndPublication);
		shaderPointerIndex = new Lazy<Dictionary<string, MetaPtr>>(BuildShaderPointerIndex, LazyThreadSafetyMode.ExecutionAndPublication);

		if (!SupportsRegistryPackages)
		{
			return;
		}

		foreach (AssemblyDefinition assembly in assemblyManager.GetAssemblies())
		{
			allAssemblies.Add(assembly.Name ?? string.Empty);
			AnalyzeAssembly(assembly);
		}

		foreach ((string packageId, Candidate candidate) in manifestCandidates)
		{
			if (!TryResolvePackageDirectory(packageId, candidate.Version, out PackageDirectoryResolution resolution))
			{
				continue;
			}
			packageDirectories[packageId] = resolution;
		}
	}

	public IAssemblyManager AssemblyManager { get; }
	public UnityVersion Version { get; }
	public string PackageCacheRoot { get; }
	public bool SupportsRegistryPackages => Version.GreaterThanOrEquals(2018, 1, 0);
	public IReadOnlyDictionary<string, string> ManifestDependencies => GetManifestCandidates().ToDictionary(static pair => pair.Key, static pair => pair.Value.Version, StringComparer.Ordinal);

	public bool TryGetScriptPointer(IMonoScript script, out MetaPtr pointer)
	{
		if (!SupportsRegistryPackages)
		{
			pointer = default;
			return false;
		}

		ScriptIdentity identity = new(script.GetAssemblyNameFixed(), script.Namespace.String, script.ClassName_R.String);
		return scriptPointerIndex.Value.TryGetValue(identity, out pointer);
	}

	public bool TryGetShaderPointer(IShader shader, out MetaPtr pointer)
	{
		if (!SupportsRegistryPackages || string.IsNullOrWhiteSpace(shader.Name))
		{
			pointer = default;
			return false;
		}

		return shaderPointerIndex.Value.TryGetValue(shader.Name, out pointer);
	}

	internal IEnumerable<PackageDetection> GetDetections()
	{
		foreach ((string key, FamilyHint hint) in familyHintsByKey.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
		{
			if (familyCandidates.TryGetValue(key, out Candidate candidate))
			{
				yield return new PackageDetection(hint.Display, hint.ManifestKey ?? hint.Family, candidate.Version, candidate.Source, candidate.Confidence, candidate.Exact);
			}
			else
			{
				yield return new PackageDetection(hint.Display, hint.ManifestKey ?? hint.Family, string.Empty, string.Empty, 0, false);
			}
		}
	}

	public string BuildVersionsReport(bool includeOtherAssemblies = true)
	{
		List<PackageDetection> detections = [.. GetDetections()];
		List<KeyValuePair<string, Candidate>> manifestDependencies = [.. GetManifestCandidates().OrderBy(static pair => pair.Key, StringComparer.Ordinal)];

		StringBuilder builder = new();
		builder.AppendLine("Official registry / UPM packages");
		if (manifestDependencies.Count == 0)
		{
			builder.AppendLine("<none>");
		}
		else
		{
			foreach ((string packageId, Candidate candidate) in manifestDependencies)
			{
				builder.AppendLine($"{packageId} = {candidate.Version} [{candidate.Source}]");
			}
		}
		builder.AppendLine();

		builder.AppendLine("Third-party exact versions");
		PackageDetection[] exactThirdPartyDetections =
		[
			.. detections.Where(static d => !string.IsNullOrWhiteSpace(d.Version) && !LooksLikePackageId(d.Id))
		];
		if (exactThirdPartyDetections.Length == 0)
		{
			builder.AppendLine("<none>");
		}
		else
		{
			foreach (PackageDetection detection in exactThirdPartyDetections)
			{
				builder.AppendLine($"{detection.Display} = {detection.Version} [{detection.Source}]");
			}
		}
		builder.AppendLine();

		builder.AppendLine("Guessed versions (based on Unity version)");
		bool foundGuess = false;
		foreach (var hint in FamilyHints.Values)
		{
			if (hint.ManifestKey != null && !manifestDependencies.Any(d => d.Key == hint.ManifestKey))
			{
				string? guess = GuessPackageVersion(hint.ManifestKey);
				if (guess != null)
				{
					builder.AppendLine($"{hint.ManifestKey} -> {guess} (Compatible with {Version})");
					foundGuess = true;
				}
			}
		}
		if (!foundGuess) builder.AppendLine("<none>");
		builder.AppendLine();

		builder.AppendLine("Detected families without exact version");
		PackageDetection[] unresolvedDetections =
		[
			.. detections.Where(static d => string.IsNullOrWhiteSpace(d.Version))
		];
		if (unresolvedDetections.Length == 0)
		{
			builder.AppendLine("<none>");
		}
		else
		{
			foreach (PackageDetection detection in unresolvedDetections)
			{
				builder.AppendLine($"{detection.Display}: family detected, exact version not exposed");
			}
		}
		builder.AppendLine();

		if (includeOtherAssemblies)
		{
			builder.AppendLine("Other detected assemblies (potential 3rd party plugins)");
			List<string> otherAssemblies = allAssemblies
				.Where(name => !name.StartsWith("Unity.", StringComparison.OrdinalIgnoreCase) && !name.StartsWith("UnityEngine.", StringComparison.OrdinalIgnoreCase))
				.Where(name => !FamilyHints.ContainsKey(name + ".dll"))
				.OrderBy(name => name, StringComparer.Ordinal)
				.ToList();

			if (otherAssemblies.Count == 0)
			{
				builder.AppendLine("<none>");
			}
			else
			{
				foreach (string name in otherAssemblies)
				{
					if (assemblyVersions.TryGetValue(name, out string? ver))
					{
						builder.AppendLine($"{name} = {ver}");
					}
					else
					{
						builder.AppendLine($"{name} (version unknown)");
					}
				}
			}
			builder.AppendLine();
		}

		builder.AppendLine("Package cache resolutions");
		if (packageDirectories.Count == 0)
		{
			builder.AppendLine("<none>");
		}
		else
		{
			foreach ((string packageId, PackageDirectoryResolution resolution) in packageDirectories.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
			{
				builder.AppendLine($"{packageId}: requested {resolution.RequestedVersion} -> {resolution.ResolvedVersion}{(resolution.ExactMatch ? " [exact]" : " [nearest-cache]")}");
			}
		}
		return builder.ToString();
	}

	private string? GuessPackageVersion(string packageId)
	{
		if (packageId == "com.unity.ugui") return "1.0.0";
		if (packageId == "com.unity.textmeshpro")
		{
			if (Version.GreaterThanOrEquals(2021, 3)) return "3.0.6";
			return "2.1.6";
		}
		if (packageId.Contains("render-pipelines.universal"))
		{
			if (Version.Major == 2022) return "14.0.11";
			if (Version.Major == 2021) return "12.1.15";
			if (Version.Major == 2020) return "10.10.1";
		}
		if (packageId.Contains("xr.management"))
		{
			if (Version.Major >= 2021) return "4.4.1";
			return "4.0.1";
		}
		return null;
	}

	private void AnalyzeAssembly(AssemblyDefinition assembly)
	{
		string assemblyName = assembly.Name ?? string.Empty;
		if (assemblyName.Length == 0)
		{
			return;
		}

		byte[] bytes = ReadAssemblyBytes(assembly);
		List<string> texts = [.. ExtractAsciiStrings(bytes), .. ExtractUtf16Strings(bytes)];
		List<(string Package, string Version)> explicitPackages = CollectExplicitPackages(texts);
		List<string> packageReferences = CollectPackageReferences(texts);
		foreach ((string packageId, string packageVersion) in explicitPackages)
		{
			AddManifestCandidate(packageId, packageVersion, "explicit_tag", 100, exact: true);
		}

		string? detectedVersion = null;
		FamilyHint? hint = TryGetFamilyHint(assemblyName, explicitPackages, packageReferences);
		
		foreach (CustomAttribute attribute in assembly.CustomAttributes)
		{
			string attributeName = attribute.Constructor?.DeclaringType?.FullName ?? string.Empty;
			List<string> stringArguments = GetStringArguments(attribute);
			if (stringArguments.Count == 0)
			{
				continue;
			}

			switch (attributeName)
			{
				case "System.Reflection.AssemblyInformationalVersionAttribute":
					foreach (string value in stringArguments)
					{
						string normalized = NormalizeVersion(value);
						if (hint is not null) AddFamilyCandidate(hint.Value, normalized, "custom_attribute", 92, exact: true);
						detectedVersion ??= normalized;
					}
					break;
				case "System.Reflection.AssemblyFileVersionAttribute":
				case "System.Reflection.AssemblyProductAttribute":
				case "System.Reflection.AssemblyDescriptionAttribute":
					foreach (string value in stringArguments)
					{
						string normalized = NormalizeVersion(value);
						if (hint is not null) AddFamilyCandidate(hint.Value, normalized, "custom_attribute", 88, exact: true);
						detectedVersion ??= normalized;
					}
					break;
				case "System.Reflection.AssemblyMetadataAttribute":
					if (stringArguments.Count >= 2 && IsVersionMetadataKey(stringArguments[0]))
					{
						string normalized = NormalizeVersion(stringArguments[1]);
						if (hint is not null) AddFamilyCandidate(hint.Value, normalized, "assembly_metadata", 90, exact: true);
						detectedVersion ??= normalized;
					}
					break;
			}
		}

		if (detectedVersion is not null)
		{
			assemblyVersions[assemblyName] = detectedVersion;
		}

		if (hint is null)
		{
			return;
		}
		FamilyHint resolvedHint = hint.Value;
		familyHintsByKey.TryAdd(resolvedHint.Family, resolvedHint);

		AddFamilyCandidate(resolvedHint, NormalizeVersion(assembly.Version?.ToString() ?? string.Empty), "assembly_version", 82, exact: true);
		foreach (string value in FindVersionsNearTokens(texts, resolvedHint.Tokens))
		{
			AddFamilyCandidate(resolvedHint, value, "context_strings", 68, exact: true);
		}
	}

	private IEnumerable<KeyValuePair<string, Candidate>> GetManifestCandidates()
	{
		foreach ((string packageId, Candidate candidate) in manifestCandidates)
		{
			if (candidate.Exact && ManifestSources.Contains(candidate.Source))
			{
				yield return new KeyValuePair<string, Candidate>(packageId, candidate);
			}
		}
	}

	private void AddManifestCandidate(string packageId, string version, string source, int confidence, bool exact)
	{
		if (!LooksLikePackageId(packageId) || !IsMeaningfulVersion(version, source))
		{
			return;
		}

		Candidate candidate = new(packageId, version, source, confidence, exact);
		if (!manifestCandidates.TryGetValue(packageId, out Candidate existing) || candidate.IsBetterThan(existing))
		{
			manifestCandidates[packageId] = candidate;
		}
	}

	private void AddFamilyCandidate(FamilyHint hint, string version, string source, int confidence, bool exact)
	{
		if (!IsMeaningfulVersion(version, source))
		{
			return;
		}

		Candidate candidate = new(hint.Family, version, source, confidence, exact);
		if (!familyCandidates.TryGetValue(hint.Family, out Candidate existing) || candidate.IsBetterThan(existing))
		{
			familyCandidates[hint.Family] = candidate;
		}

		if (hint.ManifestKey is not null)
		{
			AddManifestCandidate(hint.ManifestKey, version, source, confidence, exact);
		}
	}

	private Dictionary<ScriptIdentity, MetaPtr> BuildScriptPointerIndex()
	{
		Dictionary<ScriptIdentity, MetaPtr> index = new();
		if (!SupportsRegistryPackages || packageDirectories.Count == 0)
		{
			return index;
		}

		foreach (PackageDirectoryResolution resolution in packageDirectories.Values.OrderByDescending(static resolution => resolution.ExactMatch))
		{
			Dictionary<string, string?> asmdefCache = new(StringComparer.OrdinalIgnoreCase);
			foreach (string scriptPath in Directory.EnumerateFiles(resolution.DirectoryPath, "*.cs", SearchOption.AllDirectories))
			{
				string metaPath = scriptPath + ".meta";
				if (!File.Exists(metaPath) || !TryReadGuid(metaPath, out UnityGuid guid))
				{
					continue;
				}

				string assemblyName = GetAssemblyNameForSourceFile(scriptPath, resolution.DirectoryPath, asmdefCache);
				if (string.IsNullOrWhiteSpace(assemblyName))
				{
					continue;
				}

				string sourceText = TryReadFileText(scriptPath);
				if (sourceText.Length == 0)
				{
					continue;
				}

				string namespaceName = ExtractNamespace(sourceText);
				foreach (string typeName in ExtractTypeNames(sourceText))
				{
					index.TryAdd(new ScriptIdentity(assemblyName, namespaceName, typeName), new MetaPtr(MonoScriptFileID, guid, AssetType.Meta));
				}
			}
		}

		return index;
	}

	private Dictionary<string, MetaPtr> BuildShaderPointerIndex()
	{
		Dictionary<string, MetaPtr> index = new(StringComparer.Ordinal);
		if (!SupportsRegistryPackages || packageDirectories.Count == 0)
		{
			return index;
		}

		foreach (PackageDirectoryResolution resolution in packageDirectories.Values.OrderByDescending(static resolution => resolution.ExactMatch))
		{
			foreach (string shaderPath in Directory.EnumerateFiles(resolution.DirectoryPath, "*.shader", SearchOption.AllDirectories))
			{
				string metaPath = shaderPath + ".meta";
				if (!File.Exists(metaPath) || !TryReadGuid(metaPath, out UnityGuid guid))
				{
					continue;
				}

				string shaderText = TryReadFileText(shaderPath);
				Match match = ShaderNameRegex.Match(shaderText);
				if (!match.Success)
				{
					continue;
				}

				index.TryAdd(match.Groups[1].Value, new MetaPtr(ShaderFileID, guid, AssetType.Meta));
			}
		}

		return index;
	}

	private bool TryResolvePackageDirectory(string packageId, string requestedVersion, out PackageDirectoryResolution resolution)
	{
		string exactDirectory = Path.Combine(PackageCacheRoot, $"{packageId}@{requestedVersion}");
		if (Directory.Exists(exactDirectory))
		{
			resolution = new(packageId, requestedVersion, requestedVersion, exactDirectory, ExactMatch: true);
			return true;
		}
		if (!Directory.Exists(PackageCacheRoot))
		{
			resolution = default;
			return false;
		}

		VersionKey requestedKey = VersionKey.Parse(requestedVersion);
		PackageDirectoryResolution? best = null;
		foreach (string directory in Directory.EnumerateDirectories(PackageCacheRoot, $"{packageId}@*"))
		{
			string version = Path.GetFileName(directory)[(packageId.Length + 1)..];
			VersionKey candidateKey = VersionKey.Parse(version);
			PackageDirectoryResolution candidate = new(packageId, requestedVersion, version, directory, ExactMatch: false, candidateKey.GetSimilarityScore(requestedKey));
			if (best is null || candidate.Score > best.Value.Score || (candidate.Score == best.Value.Score && candidateKey.CompareTo(VersionKey.Parse(best.Value.ResolvedVersion)) > 0))
			{
				best = candidate;
			}
		}

		if (best is PackageDirectoryResolution value && value.Score >= 100)
		{
			resolution = value;
			return true;
		}

		resolution = default;
		return false;
	}

	private byte[] ReadAssemblyBytes(AssemblyDefinition assembly)
	{
		Stream stream = AssemblyManager.GetStreamForAssembly(assembly);
		long originalPosition = stream.CanSeek ? stream.Position : 0;
		if (stream.CanSeek)
		{
			stream.Position = 0;
		}
		try
		{
			using MemoryStream copy = new();
			stream.CopyTo(copy);
			return copy.ToArray();
		}
		finally
		{
			if (stream.CanSeek)
			{
				stream.Position = originalPosition;
			}
		}
	}

	private static List<(string Package, string Version)> CollectExplicitPackages(IEnumerable<string> texts)
	{
		List<(string Package, string Version)> results = [];
		foreach (string text in texts)
		{
			foreach (Match match in PackageTagRegex.Matches(text))
			{
				string packageId = match.Groups["pkg"].Value.ToLowerInvariant();
				string version = NormalizeVersion(match.Groups["ver"].Value);
				if (LooksLikePackageId(packageId) && !string.IsNullOrWhiteSpace(version))
				{
					results.Add((packageId, version));
				}
			}
		}
		return results;
	}

	private static List<string> CollectPackageReferences(IEnumerable<string> texts)
	{
		List<string> results = [];
		HashSet<string> seen = new(StringComparer.Ordinal);
		foreach (string text in texts)
		{
			foreach (Match match in PackageReferenceRegex.Matches(text))
			{
				string packageId = match.Groups["pkg"].Value.ToLowerInvariant();
				if (LooksLikePackageId(packageId) && packageId.Count(static c => c == '.') >= 2 && seen.Add(packageId))
				{
					results.Add(packageId);
				}
			}
		}
		return results;
	}

	private static FamilyHint? TryGetFamilyHint(string assemblyName, List<(string Package, string Version)> explicitPackages, List<string> packageReferences)
	{
		if (FamilyHints.TryGetValue(assemblyName, out FamilyHint hint))
		{
			if (packageReferences.Count == 1 && !LooksLikePackageId(hint.Family))
			{
				string packageId = packageReferences[0];
				return new FamilyHint(packageId, hint.Display, packageId, hint.Tokens);
			}
			return hint;
		}
		if (explicitPackages.Count == 1)
		{
			(string packageId, _) = explicitPackages[0];
			return new FamilyHint(packageId, packageId, packageId, [assemblyName.ToLowerInvariant()]);
		}
		if (packageReferences.Count == 1)
		{
			string packageId = packageReferences[0];
			return new FamilyHint(packageId, packageId, packageId, [assemblyName.ToLowerInvariant()]);
		}
		if (LooksLikePackageId(assemblyName))
		{
			return new FamilyHint(assemblyName, assemblyName, assemblyName, [assemblyName.ToLowerInvariant()]);
		}
		return null;
	}

	private static List<string> GetStringArguments(CustomAttribute attribute)
	{
		List<string> values = [];
		if (attribute.Signature is null)
		{
			return values;
		}
		foreach (CustomAttributeArgument argument in attribute.Signature.FixedArguments)
		{
			foreach (object? element in argument.Elements)
			{
				switch (element)
				{
					case AsmResolver.Utf8String utf8String when !string.IsNullOrEmpty(utf8String.Value):
						values.Add(utf8String.Value);
						break;
					case string text:
						values.Add(text);
						break;
				}
			}
		}
		return values;
	}

	private static IEnumerable<string> FindVersionsNearTokens(IEnumerable<string> texts, string[] tokens)
	{
		if (tokens.Length == 0)
		{
			yield break;
		}

		foreach (string text in texts)
		{
			string lower = text.ToLowerInvariant();
			if (!tokens.Any(token => lower.Contains(token, StringComparison.Ordinal)))
			{
				continue;
			}

			foreach (Match match in SemanticVersionRegex.Matches(text))
			{
				yield return NormalizeVersion(match.Value);
			}
		}
	}

	private static IEnumerable<string> ExtractAsciiStrings(byte[] data)
	{
		StringBuilder builder = new();
		foreach (byte value in data)
		{
			if (value is >= 0x20 and <= 0x7E)
			{
				builder.Append((char)value);
			}
			else
			{
				if (builder.Length >= 4)
				{
					yield return builder.ToString();
				}
				builder.Clear();
			}
		}
		if (builder.Length >= 4)
		{
			yield return builder.ToString();
		}
	}

	private static IEnumerable<string> ExtractUtf16Strings(byte[] data)
	{
		StringBuilder builder = new();
		for (int i = 0; i + 1 < data.Length; i += 2)
		{
			if (data[i + 1] == 0 && data[i] is >= 0x20 and <= 0x7E)
			{
				builder.Append((char)data[i]);
			}
			else
			{
				if (builder.Length >= 4)
				{
					yield return builder.ToString();
				}
				builder.Clear();
			}
		}
		if (builder.Length >= 4)
		{
			yield return builder.ToString();
		}
	}

	private static bool TryReadGuid(string metaPath, out UnityGuid guid)
	{
		Match match = GuidRegex.Match(TryReadFileText(metaPath));
		if (!match.Success)
		{
			guid = default;
			return false;
		}
		guid = new UnityGuid(Convert.FromHexString(match.Groups[1].Value));
		return true;
	}

	private static string GetAssemblyNameForSourceFile(string scriptPath, string packageRoot, Dictionary<string, string?> cache)
	{
		string directory = Path.GetDirectoryName(scriptPath) ?? packageRoot;
		if (cache.TryGetValue(directory, out string? cachedValue))
		{
			return cachedValue ?? string.Empty;
		}

		string? current = directory;
		while (!string.IsNullOrWhiteSpace(current) && current.StartsWith(packageRoot, StringComparison.OrdinalIgnoreCase))
		{
			string[] asmdefs = Directory.GetFiles(current, "*.asmdef", SearchOption.TopDirectoryOnly);
			if (asmdefs.Length > 0)
			{
				try
				{
					using JsonDocument document = JsonDocument.Parse(TryReadFileText(asmdefs[0]));
					if (document.RootElement.TryGetProperty("name", out JsonElement nameElement) && nameElement.ValueKind == JsonValueKind.String)
					{
						string? assemblyName = nameElement.GetString();
						cache[directory] = assemblyName;
						return assemblyName ?? string.Empty;
					}
				}
				catch
				{
				}
			}
			current = Path.GetDirectoryName(current);
		}

		cache[directory] = null;
		return string.Empty;
	}

	private static string ExtractNamespace(string sourceText) => NamespaceRegex.Match(sourceText) is Match match && match.Success ? match.Groups[1].Value : string.Empty;
	private static IEnumerable<string> ExtractTypeNames(string sourceText) => TypeNameRegex.Matches(sourceText).Select(static match => match.Groups[1].Value).Where(static name => !string.IsNullOrWhiteSpace(name)).Distinct(StringComparer.Ordinal);
	private static string TryReadFileText(string path) { try { return File.ReadAllText(path); } catch { return string.Empty; } }
	private static bool LooksLikePackageId(string value) => PackageIdRegex.IsMatch(value);
	private static bool IsVersionMetadataKey(string value) => value.Equals("version", StringComparison.OrdinalIgnoreCase) || value.Equals("packageversion", StringComparison.OrdinalIgnoreCase) || value.Equals("productversion", StringComparison.OrdinalIgnoreCase);

	private static string NormalizeVersion(string value)
	{
		value = value.Trim().Trim(',', ';', ':', '(', ')', '[', ']', '{', '}', '<', '>');
		if (value.StartsWith('v') && value.Length > 1 && char.IsDigit(value[1]))
		{
			value = value[1..];
		}
		Match match = SemanticVersionRegex.Match(value);
		return match.Success ? match.Value : value;
	}

	private static bool IsMeaningfulVersion(string version, string source)
	{
		if (string.IsNullOrWhiteSpace(version))
		{
			return false;
		}
		return version switch
		{
			"0.0.0" or "0.0.0.0" or "4.0.30319" => false,
			"1.0.0" or "1.0.0.0" => source is "explicit_tag" or "assembly_metadata",
			_ => true,
		};
	}

	private readonly record struct FamilyHint(string Family, string Display, string? ManifestKey, string[] Tokens);
	private readonly record struct Candidate(string Family, string Version, string Source, int Confidence, bool Exact)
	{
		public bool IsBetterThan(Candidate other) => Confidence != other.Confidence ? Confidence > other.Confidence : Version.Length > other.Version.Length;
	}
	public readonly record struct PackageDetection(string Display, string Id, string Version, string Source, int Confidence, bool Exact);
	private readonly record struct ScriptIdentity(string AssemblyName, string Namespace, string ClassName);
	private readonly record struct PackageDirectoryResolution(string PackageId, string RequestedVersion, string ResolvedVersion, string DirectoryPath, bool ExactMatch, int Score = int.MaxValue);
	private readonly record struct VersionKey(int Major, int Minor, int Build, int Revision) : IComparable<VersionKey>
	{
		public static VersionKey Parse(string version)
		{
			int suffixIndex = version.IndexOfAny(['-', '+']);
			string numericVersion = suffixIndex >= 0 ? version[..suffixIndex] : version;
			string[] parts = numericVersion.Split('.', StringSplitOptions.RemoveEmptyEntries);
			return new VersionKey(parts.Length > 0 ? int.Parse(parts[0]) : 0, parts.Length > 1 ? int.Parse(parts[1]) : 0, parts.Length > 2 ? int.Parse(parts[2]) : 0, parts.Length > 3 ? int.Parse(parts[3]) : 0);
		}

		public int GetSimilarityScore(VersionKey other)
		{
			int score = 0;
			if (Major == other.Major) score += 100;
			if (Minor == other.Minor) score += 25;
			if (Build == other.Build) score += 5;
			if (Revision == other.Revision) score += 1;
			return score;
		}

		public int CompareTo(VersionKey other) => Major != other.Major ? Major.CompareTo(other.Major) : Minor != other.Minor ? Minor.CompareTo(other.Minor) : Build != other.Build ? Build.CompareTo(other.Build) : Revision.CompareTo(other.Revision);
	}
}
