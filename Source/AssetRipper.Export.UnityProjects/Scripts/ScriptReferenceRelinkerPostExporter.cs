using AsmResolver.DotNet;
using AssetRipper.Assets;
using AssetRipper.Export.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.IO.Files;
using AssetRipper.Processing;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Extensions;
using System.Text;

namespace AssetRipper.Export.UnityProjects.Scripts;

public sealed class ScriptReferenceRelinkerPostExporter : IPostExporter
{
	private const string ToolRootRelativePath = "Assets/Editor/AssetRipperPatches";
	private const string MapRelativePath = ToolRootRelativePath + "/ScriptRelinkMap.tsv";
	private const string EditorScriptRelativePath = ToolRootRelativePath + "/ScriptReferenceRelinker.cs";
	private const string Header = "# guid\tfileID\tassembly\tnamespace\tclass\tfullType\tbaseType\tname";

	public void DoPostExport(GameData gameData, FullConfiguration settings, FileSystem fileSystem)
	{
		ScriptExporter exporter = new(gameData.AssemblyManager, settings);
		List<ScriptReferenceMapEntry> entries = gameData.GameBundle.FetchAssets()
			.OfType<IMonoScript>()
			.Select(script => CreateEntry(script, exporter))
			.Distinct()
			.OrderBy(entry => entry.AssemblyName, StringComparer.Ordinal)
			.ThenBy(entry => entry.Namespace, StringComparer.Ordinal)
			.ThenBy(entry => entry.ClassName, StringComparer.Ordinal)
			.ToList();

		string toolRoot = fileSystem.Path.Join(settings.ProjectRootPath, "Assets", "Editor", "AssetRipperPatches");
		fileSystem.Directory.Create(toolRoot);

		fileSystem.File.WriteAllText(fileSystem.Path.Join(settings.ProjectRootPath, MapRelativePath), BuildMap(entries), Encoding.UTF8);
		fileSystem.File.WriteAllText(fileSystem.Path.Join(settings.ProjectRootPath, EditorScriptRelativePath), EditorScriptContents, Encoding.UTF8);
	}

	private static ScriptReferenceMapEntry CreateEntry(IMonoScript script, ScriptExporter exporter)
	{
		MetaPtr pointer = exporter.CreateExportPointer(script);
		string assemblyName = NormalizeAssemblyName(script.GetAssemblyNameFixed());
		string namespaceName = script.Namespace.String;
		string className = script.ClassName_R.String;
		string fullTypeName = script.GetFullName();
		string scriptName = script.GetNonGenericClassName();

		string baseTypeName = string.Empty;
		try
		{
			if (exporter.AssemblyManager.IsSet && script.IsScriptPresents(exporter.AssemblyManager))
			{
				TypeDefinition typeDef = script.GetTypeDefinition(exporter.AssemblyManager);
				if (typeDef?.BaseType != null)
				{
					baseTypeName = typeDef.BaseType.FullName;
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Warning(LogCategory.Export, $"Failed to resolve base type for script {fullTypeName}: {ex.Message}");
		}

		return new ScriptReferenceMapEntry(
			pointer.GUID.ToString().ToLowerInvariant(),
			pointer.FileID.ToString(),
			assemblyName,
			namespaceName,
			className,
			fullTypeName.ToLowerInvariant(),
			baseTypeName,
			scriptName);
	}

	private static string BuildMap(IEnumerable<ScriptReferenceMapEntry> entries)
	{
		StringBuilder builder = new();
		builder.AppendLine(Header);
		foreach (ScriptReferenceMapEntry entry in entries)
		{
			builder
				.Append(Escape(entry.Guid)).Append('\t')
				.Append(Escape(entry.FileID)).Append('\t')
				.Append(Escape(entry.AssemblyName)).Append('\t')
				.Append(Escape(entry.Namespace)).Append('\t')
				.Append(Escape(entry.ClassName)).Append('\t')
				.Append(Escape(entry.FullTypeName)).Append('\t')
				.Append(Escape(entry.BaseTypeName)).Append('\t')
				.Append(Escape(entry.ScriptName))
				.AppendLine();
		}
		return builder.ToString();
	}

	private static string Escape(string value)
	{
		return value
			.Replace('\t', ' ')
			.Replace('\r', ' ')
			.Replace('\n', ' ')
			.Trim();
	}

	private static string NormalizeAssemblyName(string assemblyName)
	{
		string lower = assemblyName.ToLowerInvariant();
		return lower switch
		{
			"unity.addressables" => "Unity.Addressables",
			"unity.inputsystem" => "Unity.InputSystem",
			"unity.resourcegraph" => "Unity.ResourceGraph",
			"unity.renderpipelines.universal.runtime" => "Unity.RenderPipelines.Universal.Runtime",
			"unity.renderpipelines.highdefinition.runtime" => "Unity.RenderPipelines.HighDefinition.Runtime",
			"unity.renderpipelines.core.runtime" => "Unity.RenderPipelines.Core.Runtime",
			"unity.visualeffectgraph.runtime" => "Unity.VisualEffectGraph.Runtime",
			"unity.textmeshpro" => "Unity.TextMeshPro",
			"unity.mathematics" => "Unity.Mathematics",
			"unity.xr.interaction.toolkit" => "Unity.XR.Interaction.Toolkit",
			"unity.visualscripting.core" => "Unity.VisualScripting.Core",
			"unity.burst" => "Unity.Burst",
			"unity.physics" => "Unity.Physics",
			"unity.physics2d" => "Unity.Physics2D",
			"unity.ugui" => "Unity.UI",
			"unity.ai.navigation" => "Unity.AI.Navigation",
			"unity.timeline" => "Unity.Timeline",
			"unity.postprocessing.runtime" => "Unity.Postprocessing.Runtime",
			"unity.recorder" => "Unity.Recorder",
			"assembly - csharp" => "Assembly-CSharp",
			"assembly - csharp - firstpass" => "Assembly-CSharp-firstpass",
			"assembly - csharp - editor" => "Assembly-CSharp-Editor",
			"assembly - unityscript" => "Assembly-UnityScript",
			"assembly - unityscript - firstpass" => "Assembly-UnityScript-firstpass",
			_ when lower.StartsWith("assembly - ", StringComparison.Ordinal) => assemblyName.Replace(" - ", "-"),
			_ => assemblyName,
		};
	}

	private readonly record struct ScriptReferenceMapEntry(
		string Guid,
		string FileID,
		string AssemblyName,
		string Namespace,
		string ClassName,
		string FullTypeName,
		string BaseTypeName,
		string ScriptName);

	private static string EditorScriptContents =>
		"""
		#if UNITY_EDITOR
		using System;
		using System.Collections.Generic;
		using System.Globalization;
		using System.IO;
		using System.Linq;
		using System.Text;
		using System.Text.RegularExpressions;
		using UnityEditor;
		using UnityEngine;

		namespace AssetRipperPatches
		{
			internal static class ScriptReferenceRelinker
			{
				private const string MapRelativePath = "Assets/Editor/AssetRipperPatches/ScriptRelinkMap.tsv";
				private const long MonoScriptFileId = 11500000;
				private static readonly Regex PPtrReferenceRegex = new Regex(@"\{fileID:\s*(?<fileID>-?\d+),\s*guid:\s*(?<guid>[0-9a-fA-F]{32}),\s*type:\s*(?<type>\d+)\s*}", RegexOptions.Compiled);

				private static bool EnableAutoRelink = true;
				private static bool VerboseLogging = true;

				static ScriptReferenceRelinker()
				{
					if (EnableAutoRelink)
					{
						EditorApplication.delayCall += TryAutoRelink;
					}
				}

				[MenuItem("Tools/AssetRipper/Relink All References")]
				private static void RelinkFromMenu()
				{
					Relink(true, false);
				}

				[MenuItem("Tools/AssetRipper/Relink All (Dry Run)")]
				private static void RelinkDryRunFromMenu()
				{
					Relink(true, true);
				}

				[MenuItem("Assets/AssetRipper/Relink Selected Reference(s)", false, 2000)]
				private static void RelinkSelectedFromMenu()
				{
					RelinkSelected(false);
				}

				[MenuItem("Tools/AssetRipper/Recover Missing Meta Files")]
				private static void RecoverMetaFilesFromMenu()
				{
					RecoverMissingMetaFiles(true);
				}

				[MenuItem("Tools/AssetRipper/Diagnose Unresolved Scripts")]
				private static void DiagnoseFromMenu()
				{
					DiagnoseUnresolvedScripts();
				}

				[MenuItem("Tools/AssetRipper/Force Sync GUIDs from Map")]
				private static void ForceSyncFromMenu()
				{
					if (EditorUtility.DisplayDialog("Force Sync GUIDs", "This will overwrite the GUIDs in your existing .meta files to match the AssetRipper map. This cannot be easily undone. Continue?", "Yes", "Cancel"))
					{
						ForceSyncMetaFiles(true);
					}
				}

				[MenuItem("Tools/AssetRipper/Clean Unresolved References")]
				private static void CleanUnresolvedFromMenu()
				{
					if (EditorUtility.DisplayDialog("Clean Unresolved References", "This will search the entire project and null out any script references that cannot be found in the current project but exist in the AssetRipper map. Continue?", "Yes", "Cancel"))
					{
						CleanUnresolvedReferences(false);
					}
				}

				[MenuItem("Tools/AssetRipper/Clean All Unresolved (Dry Run)")]
				private static void CleanUnresolvedDryRunFromMenu()
				{
					CleanUnresolvedReferences(true);
				}

				[MenuItem("Assets/AssetRipper/Clean Selected Unresolved Reference(s)", false, 2001)]
				private static void CleanSelectedFromMenu()
				{
					CleanSelectedUnresolved(false);
				}

				private static void TryAutoRelink()
				{
					if (EditorApplication.isCompiling || EditorApplication.isUpdating)
					{
						EditorApplication.delayCall += TryAutoRelink;
						return;
					}

					RecoverMissingMetaFiles(false);
					Relink(false);
				}

				private static void RelinkSelected(bool dryRun)
				{
					string mapPath = GetAbsoluteMapPath();
					if (!File.Exists(mapPath))
					{
						Debug.LogWarning("AssetRipper relink map not found at: " + mapPath);
						return;
					}

					Dictionary<string, string> sourceMap = LoadSourceMap(mapPath);
					if (sourceMap.Count == 0) return;

					Dictionary<string, string> installedScripts = BuildInstalledScriptMap();
					string[] selectedPaths = GetSelectedAssetPaths().ToArray();

					if (selectedPaths.Length == 0)
					{
						Debug.Log("AssetRipper relink: No valid assets selected.");
						return;
					}

					int changedFiles = 0;
					int changedReferences = 0;
					HashSet<string> unresolvedIdentities = new HashSet<string>();

					try
					{
						AssetDatabase.StartAssetEditing();
						for (int i = 0; i < selectedPaths.Length; i++)
						{
							if (TryRelinkFile(selectedPaths[i], sourceMap, installedScripts, out int replacements, out int _, out int _, unresolvedIdentities, dryRun))
							{
								changedFiles++;
								changedReferences += replacements;
							}
						}
					}
					finally
					{
						AssetDatabase.StopAssetEditing();
					}

					if (changedFiles > 0)
					{
						AssetDatabase.Refresh();
						string action = dryRun ? "Would relink " : "Relinked ";
						Debug.Log("AssetRipper: " + action + changedReferences + " references in " + changedFiles + " selected assets.");
					}
					else
					{
						Debug.Log("AssetRipper: No changes needed in selected assets.");
					}
				}

				private static void Relink(bool verbose, bool dryRun = false)
				{
					string mapPath = GetAbsoluteMapPath();
					if (!File.Exists(mapPath))
					{
						if (verbose) Debug.LogWarning("AssetRipper relink map not found at: " + mapPath);
						return;
					}

					Dictionary<string, string> sourceMap = LoadSourceMap(mapPath);
					if (sourceMap.Count == 0) return;

					if (verbose || VerboseLogging) Debug.Log("AssetRipper: Starting reference relinking...");

					Dictionary<string, string> installedScripts = BuildInstalledScriptMap();

					int changedFiles = 0;
					int changedReferences = 0;
					int skippedAlreadyCorrect = 0;
					int unresolvedCount = 0;
					HashSet<string> unresolvedIdentities = new HashSet<string>();

					try
					{
						AssetDatabase.StartAssetEditing();
						string[] candidatePaths = EnumerateCandidateAssetPaths().ToArray();
						float total = candidatePaths.Length;

						for (int i = 0; i < candidatePaths.Length; i++)
						{
							string assetPath = candidatePaths[i];
							if (verbose) EditorUtility.DisplayProgressBar("Relinking Assets", assetPath, (float)i / total);

							if (TryRelinkFile(assetPath, sourceMap, installedScripts, out int replacements, out int skipped, out int unresolved, unresolvedIdentities, dryRun))
							{
								changedFiles++;
								changedReferences += replacements;
							}
							skippedAlreadyCorrect += skipped;
							unresolvedCount += unresolved;
						}
					}
					finally
					{
						AssetDatabase.StopAssetEditing();
						EditorUtility.ClearProgressBar();
					}

					if (changedFiles > 0)
					{
						AssetDatabase.Refresh();
						string action = dryRun ? "Would relink " : "Successfully relinked ";
						Debug.Log("AssetRipper " + action
							+ changedReferences.ToString(CultureInfo.InvariantCulture)
							+ " references across "
							+ changedFiles.ToString(CultureInfo.InvariantCulture)
							+ " assets. ("
							+ skippedAlreadyCorrect.ToString(CultureInfo.InvariantCulture)
							+ " already correct, left unchanged)");
					}
					else if (verbose || VerboseLogging)
					{
						Debug.Log("AssetRipper relink: No references needed updating. ("
							+ skippedAlreadyCorrect.ToString(CultureInfo.InvariantCulture)
							+ " already correct)");
					}

					if (unresolvedCount > 0 && (verbose || VerboseLogging))
					{
						StringBuilder sb = new StringBuilder();
						sb.AppendLine("AssetRipper: " + unresolvedCount.ToString(CultureInfo.InvariantCulture) + " script reference(s) could not be resolved. The following scripts were not found in the project:");
						List<string> sortedUnresolved = unresolvedIdentities.ToList();
						sortedUnresolved.Sort(StringComparer.Ordinal);
						foreach (string identity in sortedUnresolved)
						{
							sb.AppendLine("  - " + identity.Replace("|", " / "));
						}
						Debug.LogWarning(sb.ToString());
					}
				}

				/// <summary>
				/// Recovers .meta files for script assets that are missing them.
				/// This can happen if a user accidentally deletes a .meta file or
				/// if source control discards it. Without the .meta file, Unity
				/// assigns a new random GUID and all references to the script break.
				/// </summary>
				private static void RecoverMissingMetaFiles(bool verbose)
				{
					SyncMetaFiles(verbose, false);
				}

				private static void ForceSyncMetaFiles(bool verbose)
				{
					SyncMetaFiles(verbose, true);
				}

				private static void SyncMetaFiles(bool verbose, bool forceOverwrite)
				{
					string mapPath = GetAbsoluteMapPath();
					if (!File.Exists(mapPath))
					{
						if (verbose) Debug.LogWarning("AssetRipper relink map not found at: " + mapPath);
						return;
					}

					// Build a map from identity key to the expected GUID from the source map
					Dictionary<string, string> identityToSourceGuid = LoadIdentityToGuidMap(mapPath);
					if (identityToSourceGuid.Count == 0) return;

					int recovered = 0;
					int updated = 0;
					string[] scriptGuids = AssetDatabase.FindAssets("t:MonoScript");
					foreach (string guid in scriptGuids)
					{
						string assetPath = AssetDatabase.GUIDToAssetPath(guid);
						if (string.IsNullOrEmpty(assetPath)) continue;

						string fullPath = Path.GetFullPath(assetPath);
						string metaPath = fullPath + ".meta";

						bool existsOnDisk = File.Exists(metaPath);
						if (existsOnDisk && !forceOverwrite) continue;

						MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
						if (monoScript == null) continue;

						Type type = monoScript.GetClass();
						string identityKey;
						if (type != null)
						{
							string assemblyName = type.Assembly.GetName().Name ?? string.Empty;
							string fullTypeName = type.FullName ?? string.Empty;
							identityKey = MakeIdentityKey(assemblyName, fullTypeName);
						}
						else
						{
							string scriptName = monoScript.name;
							if (string.IsNullOrEmpty(scriptName)) continue;
							string dir = Path.GetDirectoryName(assetPath) ?? string.Empty;
							dir = dir.Replace('\\', '/');
							string inferredAssembly = TryInferAssemblyName(dir);
							string inferredNamespace = InferNamespace(dir);
							string fullTypeName = string.IsNullOrEmpty(inferredNamespace) ? scriptName : inferredNamespace + "." + scriptName;
							identityKey = MakeIdentityKey(inferredAssembly, fullTypeName);
						}

						if (!identityToSourceGuid.TryGetValue(identityKey, out string expectedGuid)) continue;

						if (existsOnDisk)
						{
							string metaContent = File.ReadAllText(metaPath);
							if (metaContent.Contains("guid: " + expectedGuid)) continue;

							try
							{
								string updatedContent = Regex.Replace(metaContent, @"guid:\s*[0-9a-fA-F]{32}", "guid: " + expectedGuid);
								File.WriteAllText(metaPath, updatedContent);
								updated++;
								if (verbose || VerboseLogging) Debug.Log("AssetRipper: Updated GUID in .meta for " + assetPath + " to " + expectedGuid);
							}
							catch (Exception ex)
							{
								Debug.LogWarning("AssetRipper: Failed to update .meta for " + assetPath + ": " + ex.Message);
							}
						}
						else
						{
							// Write a minimal .meta file with the correct GUID
							try
							{
								string metaContent =
									"fileFormatVersion: 2\n" +
									"guid: " + expectedGuid + "\n" +
									"MonoImporter:\n" +
									"  externalObjects: {}\n" +
									"  serializedVersion: 2\n" +
									"  defaultReferences: []\n" +
									"  executionOrder: 0\n" +
									"  icon: {instanceID: 0}\n" +
									"  userData: \n" +
									"  assetBundleName: \n" +
									"  assetBundleVariant: \n";
								File.WriteAllText(metaPath, metaContent);
								recovered++;
								if (verbose || VerboseLogging) Debug.Log("AssetRipper: Recovered .meta file for " + assetPath + " with GUID " + expectedGuid);
							}
							catch (Exception ex)
							{
								Debug.LogWarning("AssetRipper: Failed to recover .meta for " + assetPath + ": " + ex.Message);
							}
						}
					}

					if (recovered > 0 || updated > 0)
					{
						AssetDatabase.Refresh();
						if (recovered > 0) Debug.Log("AssetRipper: Recovered " + recovered.ToString(CultureInfo.InvariantCulture) + " missing .meta file(s).");
						if (updated > 0) Debug.Log("AssetRipper: Updated GUID in " + updated.ToString(CultureInfo.InvariantCulture) + " .meta file(s).");
					}
					else if (verbose)
					{
						Debug.Log("AssetRipper: No meta file changes needed.");
					}
				}

				/// <summary>
				/// Diagnoses unresolved script references by comparing the source map
				/// against currently installed scripts and reporting any mismatches.
				/// </summary>
				private static void DiagnoseUnresolvedScripts()
				{
					string mapPath = GetAbsoluteMapPath();
					if (!File.Exists(mapPath))
					{
						Debug.LogWarning("AssetRipper relink map not found at: " + mapPath);
						return;
					}

					Dictionary<string, string> sourceMap = LoadSourceMap(mapPath);
					Dictionary<string, string> installedScripts = BuildInstalledScriptMap();

					// Invert sourceMap: get all unique identity keys
					HashSet<string> allIdentities = new HashSet<string>(sourceMap.Values);
					List<string> missing = new List<string>();
					List<string> found = new List<string>();

					foreach (string identity in allIdentities)
					{
						if (installedScripts.ContainsKey(identity))
						{
							found.Add(identity);
						}
						else
						{
							missing.Add(identity);
						}
					}

					StringBuilder sb = new StringBuilder();
					sb.AppendLine("=== AssetRipper Script Reference Diagnostic ===");
					sb.AppendLine("Total unique script identities in map: " + allIdentities.Count.ToString(CultureInfo.InvariantCulture));
					sb.AppendLine("Resolved (found in project): " + found.Count.ToString(CultureInfo.InvariantCulture));
					sb.AppendLine("Unresolved (missing): " + missing.Count.ToString(CultureInfo.InvariantCulture));

					if (missing.Count > 0)
					{
						sb.AppendLine();
						sb.AppendLine("Missing scripts (Assembly / FullType):");
						missing.Sort(StringComparer.Ordinal);
						foreach (string identity in missing)
						{
							sb.AppendLine("  - " + identity.Replace("|", " / "));
						}
					}

					if (missing.Count > 0)
					{
						Debug.LogWarning(sb.ToString());
					}
					else
					{
						Debug.Log(sb.ToString());
					}
				}

				private static void CleanSelectedUnresolved(bool dryRun)
				{
					string mapPath = GetAbsoluteMapPath();
					if (!File.Exists(mapPath))
					{
						Debug.LogWarning("AssetRipper relink map not found at: " + mapPath);
						return;
					}

					Dictionary<string, string> sourceMap = LoadSourceMap(mapPath);
					Dictionary<string, string> installedScripts = BuildInstalledScriptMap();
					string[] selectedPaths = GetSelectedAssetPaths().ToArray();

					if (selectedPaths.Length == 0)
					{
						Debug.Log("AssetRipper clean: No valid assets selected.");
						return;
					}

					int cleanedFiles = 0;
					int cleanedReferences = 0;

					try
					{
						AssetDatabase.StartAssetEditing();
						for (int i = 0; i < selectedPaths.Length; i++)
						{
							if (TryCleanFile(selectedPaths[i], sourceMap, installedScripts, out int count, dryRun))
							{
								cleanedFiles++;
								cleanedReferences += count;
							}
						}
					}
					finally
					{
						AssetDatabase.StopAssetEditing();
					}

					if (cleanedFiles > 0)
					{
						AssetDatabase.Refresh();
						string action = dryRun ? "Would clean " : "Cleaned ";
						Debug.Log("AssetRipper: " + action + cleanedReferences + " references in " + cleanedFiles + " selected assets.");
					}
					else
					{
						Debug.Log("AssetRipper: No unresolved references found in selected assets.");
					}
				}

				private static void CleanUnresolvedReferences(bool dryRun)
				{
					string mapPath = GetAbsoluteMapPath();
					if (!File.Exists(mapPath))
					{
						Debug.LogWarning("AssetRipper relink map not found at: " + mapPath);
						return;
					}

					Dictionary<string, string> sourceMap = LoadSourceMap(mapPath);
					Dictionary<string, string> installedScripts = BuildInstalledScriptMap();

					int cleanedFiles = 0;
					int cleanedReferences = 0;

					try
					{
						AssetDatabase.StartAssetEditing();
						string[] candidatePaths = EnumerateCandidateAssetPaths().ToArray();
						float total = candidatePaths.Length;

						for (int i = 0; i < candidatePaths.Length; i++)
						{
							string assetPath = candidatePaths[i];
							EditorUtility.DisplayProgressBar("Cleaning Unresolved References", assetPath, (float)i / total);

							if (TryCleanFile(assetPath, sourceMap, installedScripts, out int count, dryRun))
							{
								cleanedFiles++;
								cleanedReferences += count;
							}
						}
					}
					finally
					{
						AssetDatabase.StopAssetEditing();
						EditorUtility.ClearProgressBar();
					}

					if (cleanedFiles > 0)
					{
						AssetDatabase.Refresh();
						string action = dryRun ? "Would clean " : "Successfully cleaned ";
						Debug.Log("AssetRipper " + action + cleanedReferences.ToString(CultureInfo.InvariantCulture) + " unresolved script references across " + cleanedFiles.ToString(CultureInfo.InvariantCulture) + " assets.");
					}
					else
					{
						Debug.Log("AssetRipper clean: No unresolved references found to clean.");
					}
				}

				private static bool TryCleanFile(string assetPath, Dictionary<string, string> sourceMap, Dictionary<string, string> installedScripts, out int cleanedCount, bool dryRun = false)
				{
					cleanedCount = 0;
					string fullPath = Path.GetFullPath(assetPath);
					string originalText;
					try { originalText = File.ReadAllText(fullPath); }
					catch { return false; }

					int localCleaned = 0;
					string updatedText = PPtrReferenceRegex.Replace(originalText, match =>
					{
						if (!long.TryParse(match.Groups["fileID"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long fileId)) return match.Value;
						string guid = match.Groups["guid"].Value.ToLowerInvariant();
						string sourceKey = MakeSourceKey(guid, fileId);
						if (!sourceMap.TryGetValue(sourceKey, out string identityKey)) return match.Value;
						if (installedScripts.ContainsKey(identityKey)) return match.Value;

						localCleaned++;
						return "{fileID: 0}";
					});

					cleanedCount = localCleaned;
					if (localCleaned == 0 || string.Equals(originalText, updatedText, StringComparison.Ordinal)) return false;
					if (dryRun) return true;

					try
					{
						File.WriteAllText(fullPath, updatedText);
						return true;
					}
					catch { return false; }
				}

				private static bool TryRelinkFile(
					string assetPath,
					Dictionary<string, string> sourceMap,
					Dictionary<string, string> installedScripts,
					out int replacements,
					out int skipped,
					out int unresolved,
					HashSet<string> unresolvedIdentities,
					bool dryRun = false)
				{
					replacements = 0;
					skipped = 0;
					unresolved = 0;

					string fullPath = Path.GetFullPath(assetPath);
					string originalText;
					try { originalText = File.ReadAllText(fullPath); }
					catch { return false; }

					int localReplacements = 0;
					int localSkipped = 0;
					int localUnresolved = 0;
					string updatedText = PPtrReferenceRegex.Replace(originalText, match =>
					{
						if (!long.TryParse(match.Groups["fileID"].Value, NumberStyles.Integer,
								CultureInfo.InvariantCulture, out long fileId))
						{
							return match.Value;
						}

						string guid = match.Groups["guid"].Value.ToLowerInvariant();

						string sourceKey = MakeSourceKey(guid, fileId);
						if (!sourceMap.TryGetValue(sourceKey, out string identityKey))
						{
							return match.Value;
						}

						if (!installedScripts.TryGetValue(identityKey, out string installedGuid))
						{
							localUnresolved++;
							if (unresolvedIdentities != null)
							{
								unresolvedIdentities.Add(identityKey);
							}
							return match.Value;
						}

						if (string.Equals(installedGuid, guid, StringComparison.OrdinalIgnoreCase)
							&& fileId == MonoScriptFileId)
						{
							localSkipped++;
							return match.Value;
						}

						localReplacements++;
						return "{fileID: "
							+ MonoScriptFileId.ToString(CultureInfo.InvariantCulture)
							+ ", guid: " + installedGuid
							+ ", type: " + match.Groups["type"].Value
							+ "}";
					});

					replacements = localReplacements;
					skipped = localSkipped;
					unresolved = localUnresolved;

					if (localReplacements == 0
						|| string.Equals(originalText, updatedText, StringComparison.Ordinal))
					{
						return false;
					}

					if (dryRun) return true;

					try
					{
						File.WriteAllText(fullPath, updatedText);
						return true;
					}
					catch { return false; }
				}

				private static Dictionary<string, string> LoadSourceMap(string mapPath)
				{
					Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.Ordinal);
					foreach (string rawLine in File.ReadAllLines(mapPath))
					{
						string line = rawLine.Trim();
						if (line.Length == 0 || line[0] == '#') continue;

						string[] parts = line.Split('\t');
						if (parts.Length < 6) continue;

						if (!long.TryParse(parts[1], NumberStyles.Integer,
								CultureInfo.InvariantCulture, out long fileId))
						{
							continue;
						}

						// Key: Assembly + FullTypeName (column 2 and 5)
						map[MakeSourceKey(parts[0], fileId)] = MakeIdentityKey(parts[2], parts[5]);
					}
					return map;
				}

				private static Dictionary<string, string> LoadIdentityToGuidMap(string mapPath)
				{
					Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.Ordinal);
					foreach (string rawLine in File.ReadAllLines(mapPath))
					{
						string line = rawLine.Trim();
						if (line.Length == 0 || line[0] == '#') continue;

						string[] parts = line.Split('\t');
						if (parts.Length < 6) continue;

						string guid = parts[0].Trim().ToLowerInvariant();
						// Key: Assembly + FullTypeName
						string identityKey = MakeIdentityKey(parts[2], parts[5]);
						if (!map.ContainsKey(identityKey))
						{
							map[identityKey] = guid;
						}
					}
					return map;
				}

				private static Dictionary<string, string> BuildInstalledScriptMap()
				{
					Dictionary<string, string> installedScripts = new Dictionary<string, string>(StringComparer.Ordinal);
					string[] guids = AssetDatabase.FindAssets("t:MonoScript");
					foreach (string guid in guids)
					{
						string path = AssetDatabase.GUIDToAssetPath(guid);
						MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
						if (monoScript == null) continue;

						Type type = monoScript.GetClass();
						if (type != null)
						{
							string assemblyName = type.Assembly.GetName().Name ?? string.Empty;
							string fullTypeName = type.FullName ?? string.Empty;
							string key = MakeIdentityKey(assemblyName, fullTypeName);
							if (!installedScripts.ContainsKey(key))
							{
								installedScripts[key] = guid.ToLowerInvariant();
							}
						}
						else
						{
							// Fallback: GetClass() returns null for scripts that failed to compile
							// or for MonoBehaviours/ScriptableObjects in Unity 2019+ that have not
							// been resolved yet. Try to extract identity from the file name and path.
							string scriptName = monoScript.name;
							if (string.IsNullOrEmpty(scriptName)) continue;

							// Attempt to infer the namespace from the directory structure
							string dir = Path.GetDirectoryName(path) ?? string.Empty;
							dir = dir.Replace('\\', '/');

							// Look for assembly name from .asmdef files in parent directories
							string inferredAssembly = TryInferAssemblyName(dir);
							string inferredNamespace = InferNamespace(dir);

							string fullTypeName = string.IsNullOrEmpty(inferredNamespace) ? scriptName : inferredNamespace + "." + scriptName;
							string key = MakeIdentityKey(inferredAssembly, fullTypeName);
							if (!installedScripts.ContainsKey(key))
							{
								installedScripts[key] = guid.ToLowerInvariant();
							}
						}
					}
					return installedScripts;
				}

				private static string InferNamespace(string directory)
				{
					directory = directory.Replace('\\', '/');
					if (!directory.EndsWith("/")) directory += "/";

					int scriptsIdx = directory.IndexOf("/Scripts/", StringComparison.OrdinalIgnoreCase);
					if (scriptsIdx >= 0)
					{
						string sub = directory.Substring(scriptsIdx + 9);
						int nextSlash = sub.IndexOf('/');
						if (nextSlash >= 0)
						{
							return sub.Substring(nextSlash + 1).Trim('/').Replace('/', '.');
						}
						return "";
					}

					int pluginsIdx = directory.IndexOf("/Plugins/", StringComparison.OrdinalIgnoreCase);
					if (pluginsIdx >= 0)
					{
						string sub = directory.Substring(pluginsIdx + 9);
						int nextSlash = sub.IndexOf('/');
						if (nextSlash >= 0)
						{
							return sub.Substring(nextSlash + 1).Trim('/').Replace('/', '.');
						}
						return "";
					}

					return "";
				}

				/// <summary>
				/// Attempts to infer the assembly name for a script by searching for .asmdef files
				/// in the script's directory and parent directories. Falls back to "Assembly-CSharp".
				/// </summary>
				private static string TryInferAssemblyName(string directory)
				{
					try
					{
						string dir = directory.Replace('\\', '/');
						if (!dir.EndsWith("/")) dir += "/";

						// Try to infer from AssetRipper's export structure: Assets/Scripts/{AssemblyName}/...
						int scriptsIdx = dir.IndexOf("/Scripts/", StringComparison.OrdinalIgnoreCase);
						if (scriptsIdx >= 0)
						{
							string sub = dir.Substring(scriptsIdx + 9);
							int nextSlash = sub.IndexOf('/');
							if (nextSlash >= 0)
							{
								return sub.Substring(0, nextSlash);
							}
						}

						int pluginsIdx = dir.IndexOf("/Plugins/", StringComparison.OrdinalIgnoreCase);
						if (pluginsIdx >= 0)
						{
							string sub = dir.Substring(pluginsIdx + 9);
							int nextSlash = sub.IndexOf('/');
							if (nextSlash >= 0)
							{
								return sub.Substring(0, nextSlash);
							}
						}

						while (!string.IsNullOrEmpty(dir))
						{
							dir = dir.TrimEnd('/');
							// Don't search outside the exported project folders
							if (!dir.StartsWith("Assets", StringComparison.OrdinalIgnoreCase)
								&& !dir.StartsWith("Packages", StringComparison.OrdinalIgnoreCase))
							{
								break;
							}

							string[] asmdefFiles = Directory.GetFiles(dir, "*.asmdef", SearchOption.TopDirectoryOnly);
							if (asmdefFiles.Length > 0)
							{
								// The asmdef file name (without extension) is typically the assembly name
								return Path.GetFileNameWithoutExtension(asmdefFiles[0]);
							}
							string parent = Path.GetDirectoryName(dir);
							if (string.IsNullOrEmpty(parent) || string.Equals(parent, dir, StringComparison.OrdinalIgnoreCase)) break;
							dir = parent;
						}
					}
					catch
					{
						// Silently fall back
					}
					return "Assembly-CSharp";
				}

				private static IEnumerable<string> GetSelectedAssetPaths()
				{
					string[] guids = Selection.assetGUIDs;
					HashSet<string> extensions = new HashSet<string>(GetCandidateExtensions(), StringComparer.OrdinalIgnoreCase);

					foreach (string guid in guids)
					{
						string path = AssetDatabase.GUIDToAssetPath(guid);
						if (string.IsNullOrEmpty(path)) continue;

						if (Directory.Exists(path))
						{
							foreach (string subPath in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
							{
								string ext = Path.GetExtension(subPath);
								if (extensions.Contains("*" + ext))
								{
									yield return subPath.Replace('\\', '/');
								}
							}
						}
						else
						{
							string ext = Path.GetExtension(path);
							if (extensions.Contains("*" + ext))
							{
								yield return path.Replace('\\', '/');
							}
						}
					}
				}

				private static string[] GetCandidateExtensions()
				{
					return new[]
					{
						"*.prefab", "*.unity", "*.asset", "*.anim",
						"*.controller", "*.overrideController",
						"*.playable", "*.mat", "*.mask",
						"*.signal", "*.lighting", "*.flare",
						"*.mixer", "*.renderTexture",
						"*.shadervariants", "*.terrainlayer",
						"*.fontsettings", "*.guiskin", "*.brush",
						"*.physicMaterial", "*.physicsMaterial2D", "*.font", "*.vfx", "*.spriteatlas",
						"*.inputactions", "*.spriteatlasv2", "*.computeShader",
						"*.shadergraph", "*.subgraph", "*.visualelements", "*.uss",
						"*.uxml", "*.razor"
					};
				}

				private static IEnumerable<string> EnumerateCandidateAssetPaths()
				{
					HashSet<string> extensions = new HashSet<string>(GetCandidateExtensions(), StringComparer.OrdinalIgnoreCase);

					if (Directory.Exists("Assets"))
					{
						foreach (string fullPath in Directory.EnumerateFiles("Assets", "*.*", SearchOption.AllDirectories))
						{
							string normalizedPath = fullPath.Replace('\\', '/');
							if (normalizedPath.Contains("/Editor/AssetRipperPatches/")) continue;
							if (extensions.Contains("*" + Path.GetExtension(normalizedPath)))
							{
								yield return normalizedPath;
							}
						}
					}

					if (Directory.Exists("Packages"))
					{
						foreach (string fullPath in Directory.EnumerateFiles("Packages", "*.*", SearchOption.AllDirectories))
						{
							string normalizedPath = fullPath.Replace('\\', '/');
							if (extensions.Contains("*" + Path.GetExtension(normalizedPath)))
							{
								yield return normalizedPath;
							}
						}
					}
				}

				private static string MakeSourceKey(string guid, long fileId)
				{
					return guid.ToLowerInvariant() + "|" + fileId.ToString(CultureInfo.InvariantCulture);
				}

				private static string MakeIdentityKey(string assemblyName, string fullTypeName)
				{
					return NormalizeAssemblyName(assemblyName) + "|" + fullTypeName.ToLowerInvariant();
				}

				private static string NormalizeAssemblyName(string assemblyName)
				{
					string lower = assemblyName.ToLowerInvariant();
					switch (lower)
					{
						case "unity.addressables": return "Unity.Addressables";
						case "unity.inputsystem": return "Unity.InputSystem";
						case "unity.resourcegraph": return "Unity.ResourceGraph";
						case "unity.renderpipelines.universal.runtime": return "Unity.RenderPipelines.Universal.Runtime";
						case "unity.renderpipelines.highdefinition.runtime": return "Unity.RenderPipelines.HighDefinition.Runtime";
						case "unity.renderpipelines.core.runtime": return "Unity.RenderPipelines.Core.Runtime";
						case "unity.visualeffectgraph.runtime": return "Unity.VisualEffectGraph.Runtime";
						case "unity.textmeshpro": return "Unity.TextMeshPro";
						case "unity.mathematics": return "Unity.Mathematics";
						case "unity.xr.interaction.toolkit": return "Unity.XR.Interaction.Toolkit";
						case "unity.visualscripting.core": return "Unity.VisualScripting.Core";
						case "unity.burst": return "Unity.Burst";
						case "unity.physics": return "Unity.Physics";
						case "unity.physics2d": return "Unity.Physics2D";
						case "unity.ugui": return "Unity.UI";
						case "unity.ai.navigation": return "Unity.AI.Navigation";
						case "unity.timeline": return "Unity.Timeline";
						case "unity.postprocessing.runtime": return "Unity.Postprocessing.Runtime";
						case "unity.recorder": return "Unity.Recorder";
						case "assembly - csharp": return "Assembly-CSharp";
						case "assembly - csharp - firstpass": return "Assembly-CSharp-firstpass";
						case "assembly - csharp - editor": return "Assembly-CSharp-Editor";
						case "assembly - unityscript": return "Assembly-UnityScript";
						case "assembly - unityscript - firstpass": return "Assembly-UnityScript-firstpass";
						default:
							if (lower.StartsWith("assembly - ")) return assemblyName.Replace(" - ", "-");
							return assemblyName;
					}
				}

				private static string GetAbsoluteMapPath()
				{
					return Path.GetFullPath(MapRelativePath);
				}
			}
		}
		#endif
		""";
}
