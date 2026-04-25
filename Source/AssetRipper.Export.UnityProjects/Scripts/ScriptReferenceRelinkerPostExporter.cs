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
		string assemblyName = script.GetAssemblyNameFixed();
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
			fullTypeName,
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
					Relink(true);
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

				private static void Relink(bool verbose)
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

							if (TryRelinkFile(assetPath, sourceMap, installedScripts, out int replacements, out int skipped, out int unresolved, unresolvedIdentities))
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
						Debug.Log("AssetRipper successfully relinked "
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
						foreach (string identity in unresolvedIdentities)
						{
							sb.AppendLine("  - " + identity.Replace("\t", " / "));
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
					string[] scriptGuids = AssetDatabase.FindAssets("t:MonoScript");
					foreach (string guid in scriptGuids)
					{
						string assetPath = AssetDatabase.GUIDToAssetPath(guid);
						if (string.IsNullOrEmpty(assetPath)) continue;

						string fullPath = Path.GetFullPath(assetPath);
						string metaPath = fullPath + ".meta";

						// Only recover if the .meta file is truly missing on disk
						// (AssetDatabase may have auto-generated a temporary one)
						if (File.Exists(metaPath)) continue;

						MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
						if (monoScript == null) continue;

						Type type = monoScript.GetClass();
						if (type == null) continue;

						string assemblyName = type.Assembly.GetName().Name ?? string.Empty;
						string namespaceName = type.Namespace ?? string.Empty;
						string className = type.Name;
						string identityKey = MakeIdentityKey(assemblyName, namespaceName, className);

						if (!identityToSourceGuid.TryGetValue(identityKey, out string expectedGuid)) continue;

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
							if (verbose || VerboseLogging)
							{
								Debug.Log("AssetRipper: Recovered .meta file for " + assetPath + " with GUID " + expectedGuid);
							}
						}
						catch (Exception ex)
						{
							Debug.LogWarning("AssetRipper: Failed to recover .meta for " + assetPath + ": " + ex.Message);
						}
					}

					if (recovered > 0)
					{
						AssetDatabase.Refresh();
						Debug.Log("AssetRipper: Recovered " + recovered.ToString(CultureInfo.InvariantCulture) + " missing .meta file(s).");
					}
					else if (verbose)
					{
						Debug.Log("AssetRipper: No missing .meta files detected.");
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
						sb.AppendLine("Missing scripts (Assembly / Namespace / Class):");
						missing.Sort(StringComparer.Ordinal);
						foreach (string identity in missing)
						{
							sb.AppendLine("  - " + identity.Replace("\t", " / "));
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

				private static bool TryRelinkFile(
					string assetPath,
					Dictionary<string, string> sourceMap,
					Dictionary<string, string> installedScripts,
					out int replacements,
					out int skipped,
					out int unresolved,
					HashSet<string> unresolvedIdentities)
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

						string[] parts = rawLine.Split('\t');
						if (parts.Length < 5) continue;

						if (!long.TryParse(parts[1], NumberStyles.Integer,
								CultureInfo.InvariantCulture, out long fileId))
						{
							continue;
						}

						map[MakeSourceKey(parts[0], fileId)] = MakeIdentityKey(parts[2], parts[3], parts[4]);
					}
					return map;
				}

				/// <summary>
				/// Builds a map from identity key (assembly+namespace+class) to the source GUID.
				/// Used for .meta file recovery to know what GUID a script should have.
				/// </summary>
				private static Dictionary<string, string> LoadIdentityToGuidMap(string mapPath)
				{
					Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.Ordinal);
					foreach (string rawLine in File.ReadAllLines(mapPath))
					{
						string line = rawLine.Trim();
						if (line.Length == 0 || line[0] == '#') continue;

						string[] parts = rawLine.Split('\t');
						if (parts.Length < 5) continue;

						string guid = parts[0].Trim().ToLowerInvariant();
						string identityKey = MakeIdentityKey(parts[2], parts[3], parts[4]);
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
							string namespaceName = type.Namespace ?? string.Empty;
							string className = type.Name;
							string key = MakeIdentityKey(assemblyName, namespaceName, className);
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

							// Use an empty namespace as fallback
							string key = MakeIdentityKey(inferredAssembly, string.Empty, scriptName);
							if (!installedScripts.ContainsKey(key))
							{
								installedScripts[key] = guid.ToLowerInvariant();
							}
						}
					}
					return installedScripts;
				}

				/// <summary>
				/// Attempts to infer the assembly name for a script by searching for .asmdef files
				/// in the script's directory and parent directories. Falls back to "Assembly-CSharp".
				/// </summary>
				private static string TryInferAssemblyName(string directory)
				{
					try
					{
						string dir = directory;
						while (!string.IsNullOrEmpty(dir))
						{
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
							if (string.Equals(parent, dir, StringComparison.OrdinalIgnoreCase)) break;
							dir = parent;
						}
					}
					catch
					{
						// Silently fall back
					}
					return "Assembly-CSharp";
				}

				private static IEnumerable<string> EnumerateCandidateAssetPaths()
				{
					string[] extensions =
					{
						"*.prefab", "*.unity", "*.asset", "*.anim",
						"*.controller", "*.overrideController",
						"*.playable", "*.mat", "*.mask",
						"*.signal", "*.lighting", "*.flare",
						"*.mixer", "*.renderTexture",
						"*.shadervariants", "*.terrainlayer",
						"*.fontsettings", "*.guiskin", "*.brush"
					};

					// Search Assets/ folder
					if (Directory.Exists("Assets"))
					{
						foreach (string extension in extensions)
						{
							foreach (string fullPath in Directory.GetFiles("Assets", extension, SearchOption.AllDirectories))
							{
								yield return fullPath.Replace('\\', '/');
							}
						}
					}

					// Search Packages/ folder (Unity 2018.1+)
					// Embedded and local packages may contain assets with script references
					if (Directory.Exists("Packages"))
					{
						foreach (string extension in extensions)
						{
							foreach (string fullPath in Directory.GetFiles("Packages", extension, SearchOption.AllDirectories))
							{
								yield return fullPath.Replace('\\', '/');
							}
						}
					}
				}

				private static string MakeSourceKey(string guid, long fileId)
				{
					return guid.ToLowerInvariant() + "|" + fileId.ToString(CultureInfo.InvariantCulture);
				}

				private static string MakeIdentityKey(string assemblyName, string namespaceName, string className)
				{
					return assemblyName + "\t" + namespaceName + "\t" + className;
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
