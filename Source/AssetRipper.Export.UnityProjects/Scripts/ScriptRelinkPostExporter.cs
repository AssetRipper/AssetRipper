using AsmResolver.DotNet;
using AssetRipper.Export.Configuration;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Processing;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using System.Text;

namespace AssetRipper.Export.UnityProjects.Scripts;

public sealed class ScriptRelinkPostExporter : IPostExporter
{
	private const string MapFileName = "ScriptRelinkMap.tsv";

	public void DoPostExport(GameData gameData, FullConfiguration settings, FileSystem fileSystem)
	{
		RegistryPackageBridge registryPackageBridge = new(gameData.AssemblyManager, settings.Version);
		ScriptExporter scriptExporter = new(gameData.AssemblyManager, settings, registryPackageBridge);
		List<ScriptLinkEntry> entries = BuildEntries(gameData, scriptExporter);
		if (entries.Count == 0)
		{
			return;
		}

		string patchDirectory = fileSystem.Path.Join(settings.AssetsPath, "Editor", "AssetRipperPatches");
		fileSystem.Directory.Create(patchDirectory);
		fileSystem.File.WriteAllText(fileSystem.Path.Join(patchDirectory, MapFileName), BuildMapFile(entries));

		UnityPatches.ApplyPatchFromText(EditorPatchText, "ScriptReferenceRelinker", settings.ProjectRootPath, fileSystem);
	}

	private static List<ScriptLinkEntry> BuildEntries(GameData gameData, ScriptExporter scriptExporter)
	{
		Dictionary<ScriptReferenceKey, ScriptLinkEntry> entries = new();
		foreach (IMonoScript script in gameData.GameBundle.FetchAssets().OfType<IMonoScript>())
		{
			MonoScriptInfo info = MonoScriptInfo.From(script);
			if (info.IsInjected())
			{
				continue;
			}

			string baseType = "";
			try
			{
				TypeDefinition typeDef = script.GetTypeDefinition(gameData.AssemblyManager);
				baseType = typeDef.BaseType?.FullName ?? "";
			}
			catch { }

			MetaPtr pointer = scriptExporter.CreateExportPointer(script);
			ScriptReferenceKey key = new(pointer.GUID.ToString().ToLowerInvariant(), pointer.FileID);
			entries.TryAdd(key, new ScriptLinkEntry(key.Guid, key.FileID, info.Assembly, info.Namespace, info.Class, baseType));
		}

		return entries.Values
			.OrderBy(entry => entry.Assembly, StringComparer.Ordinal)
			.ThenBy(entry => entry.Namespace, StringComparer.Ordinal)
			.ThenBy(entry => entry.Class, StringComparer.Ordinal)
			.ToList();
	}

	private static string BuildMapFile(IEnumerable<ScriptLinkEntry> entries)
	{
		StringBuilder builder = new();
		builder.AppendLine("# guid\tfileID\tassembly\tnamespace\tclass\tfullType\tbaseType");
		foreach (ScriptLinkEntry entry in entries)
		{
			builder.Append(entry.Guid);
			builder.Append('\t');
			builder.Append(entry.FileID);
			builder.Append('\t');
			builder.Append(entry.Assembly);
			builder.Append('\t');
			builder.Append(entry.Namespace);
			builder.Append('\t');
			builder.Append(entry.Class);
			builder.Append('\t');
			if (string.IsNullOrEmpty(entry.Namespace))
			{
				builder.Append(entry.Class);
			}
			else
			{
				builder.Append($"{entry.Namespace}.{entry.Class}");
			}
			builder.Append('\t');
			builder.AppendLine(entry.BaseType);
		}
		return builder.ToString();
	}

	private readonly record struct ScriptReferenceKey(string Guid, long FileID);
	private readonly record struct ScriptLinkEntry(string Guid, long FileID, string Assembly, string Namespace, string Class, string BaseType);

	private const string EditorPatchText = """
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace AssetRipperPatches
{
	internal static class ScriptReferenceRelinker
	{
		private const string MapRelativePath = "Assets/Editor/AssetRipperPatches/ScriptRelinkMap.tsv";
		private const long MonoScriptFileId = 11500000;
		private static readonly Regex ScriptReferenceRegex = new Regex(@"m_Script:\s*\{fileID:\s*(-?\d+),\s*guid:\s*([0-9a-fA-F]{32}),\s*type:\s*(\d+)\s*\}", RegexOptions.Compiled);

		static ScriptReferenceRelinker()
		{
			EditorApplication.delayCall += TryAutoRelink;
		}

		[MenuItem("Tools/AssetRipper/Relink Installed Scripts")]
		private static void RelinkFromMenu()
		{
			Relink(true);
		}

		private static void TryAutoRelink()
		{
			if (EditorApplication.isCompiling || EditorApplication.isUpdating)
			{
				EditorApplication.delayCall += TryAutoRelink;
				return;
			}

			Relink(false);
		}

		private static void Relink(bool verbose)
		{
			string mapPath = GetAbsoluteMapPath();
			if (!File.Exists(mapPath))
			{
				return;
			}

			Dictionary<string, string> sourceMap = LoadSourceMap(mapPath);
			if (sourceMap.Count == 0)
			{
				return;
			}

			Dictionary<string, string> installedScripts = BuildInstalledScriptMap();
			if (installedScripts.Count == 0)
			{
				if (verbose)
				{
					Debug.Log("AssetRipper relink: no installed MonoScript assets were found yet.");
				}
				return;
			}

			int changedFiles = 0;
			int changedReferences = 0;

			foreach (string assetPath in EnumerateCandidateAssetPaths())
			{
				if (!TryRelinkFile(assetPath, sourceMap, installedScripts, out int replacements))
				{
					continue;
				}

				changedFiles++;
				changedReferences += replacements;
			}

			if (changedFiles > 0)
			{
				AssetDatabase.Refresh();
				Debug.Log(string.Format(CultureInfo.InvariantCulture, "AssetRipper relinked {0} script references across {1} assets.", changedReferences, changedFiles));
			}
			else if (verbose)
			{
				Debug.Log("AssetRipper relink did not find any script references that needed updating.");
			}
		}

		private static bool TryRelinkFile(string assetPath, Dictionary<string, string> sourceMap, Dictionary<string, string> installedScripts, out int replacements)
		{
			replacements = 0;

			string fullPath = Path.GetFullPath(assetPath);
			string originalText;
			try
			{
				originalText = File.ReadAllText(fullPath);
			}
			catch
			{
				return false;
			}

			if (originalText.IndexOf("m_Script:", StringComparison.Ordinal) < 0)
			{
				return false;
			}

			string updatedText = ScriptReferenceRegex.Replace(originalText, match =>
			{
				if (!long.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long fileId))
				{
					return match.Value;
				}

				string guid = match.Groups[2].Value.ToLowerInvariant();
				string sourceKey = MakeSourceKey(guid, fileId);
				if (!sourceMap.TryGetValue(sourceKey, out string? identityKey))
				{
					return match.Value;
				}

				if (!TryResolveInstalledGuid(installedScripts, identityKey, out string? installedGuid))
				{
					return match.Value;
				}

				if (string.Equals(installedGuid, guid, StringComparison.OrdinalIgnoreCase) && fileId == MonoScriptFileId)
				{
					return match.Value;
				}

				replacements++;
				return string.Format(CultureInfo.InvariantCulture, "m_Script: {{fileID: {0}, guid: {1}, type: {2}}}", MonoScriptFileId, installedGuid, match.Groups[3].Value);
			});

			if (replacements == 0 || string.Equals(originalText, updatedText, StringComparison.Ordinal))
			{
				return false;
			}

			File.WriteAllText(fullPath, updatedText);
			return true;
		}

		private static Dictionary<string, string> LoadSourceMap(string mapPath)
		{
			Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.Ordinal);
			foreach (string rawLine in File.ReadAllLines(mapPath))
			{
				string line = rawLine.Trim();
				if (line.Length == 0 || line[0] == '#')
				{
					continue;
				}

				string[] parts = rawLine.Split('\t');
				if (parts.Length < 5)
				{
					continue;
				}

				if (!long.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out long fileId))
				{
					continue;
				}

				string sourceKey = MakeSourceKey(parts[0], fileId);
				string identityKey = MakeIdentityKey(parts[2], parts[3], parts[4]);
				map[sourceKey] = identityKey;
			}

			return map;
		}

		private static Dictionary<string, string> BuildInstalledScriptMap()
		{
			Dictionary<string, string> installedScripts = new Dictionary<string, string>(StringComparer.Ordinal);
			Dictionary<string, string> namespaceAndClassMatches = new Dictionary<string, string>(StringComparer.Ordinal);
			HashSet<string> ambiguousNamespaceAndClassMatches = new HashSet<string>(StringComparer.Ordinal);
			Dictionary<string, string> classMatches = new Dictionary<string, string>(StringComparer.Ordinal);
			HashSet<string> ambiguousClassMatches = new HashSet<string>(StringComparer.Ordinal);
			HashSet<string> visitedGuids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (string root in new[] { "Assets", "Packages" })
			{
				foreach (string guid in AssetDatabase.FindAssets("t:MonoScript", new[] { root }))
				{
					if (!visitedGuids.Add(guid))
					{
						continue;
					}

					string path = AssetDatabase.GUIDToAssetPath(guid);
					MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
					if (monoScript == null)
					{
						continue;
					}

					Type type = monoScript.GetClass();
					if (type == null)
					{
						continue;
					}

					string assemblyName = type.Assembly.GetName().Name ?? string.Empty;
					string identityKey = MakeIdentityKey(assemblyName, type.Namespace ?? string.Empty, type.Name);
					string installedGuid = guid.ToLowerInvariant();
					installedScripts[MakeExactLookupKey(identityKey)] = installedGuid;
					RegisterFallback(namespaceAndClassMatches, ambiguousNamespaceAndClassMatches, MakeNamespaceAndClassKey(type.Namespace ?? string.Empty, type.Name), installedGuid);
					RegisterFallback(classMatches, ambiguousClassMatches, MakeClassKey(type.Name), installedGuid);
				}
			}

			foreach (KeyValuePair<string, string> pair in namespaceAndClassMatches)
			{
				installedScripts[MakeNamespaceAndClassLookupKey(pair.Key)] = pair.Value;
			}

			foreach (KeyValuePair<string, string> pair in classMatches)
			{
				installedScripts[MakeClassLookupKey(pair.Key)] = pair.Value;
			}

			return installedScripts;
		}

		private static IEnumerable<string> EnumerateCandidateAssetPaths()
		{
			if (!Directory.Exists("Assets"))
			{
				yield break;
			}

			foreach (string extension in new[] { "*.prefab", "*.unity", "*.asset" })
			{
				foreach (string fullPath in Directory.GetFiles("Assets", extension, SearchOption.AllDirectories))
				{
					yield return fullPath.Replace('\\', '/');
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

		private static bool TryResolveInstalledGuid(Dictionary<string, string> installedScripts, string identityKey, out string installedGuid)
		{
			if (installedScripts.TryGetValue(MakeExactLookupKey(identityKey), out installedGuid))
			{
				return true;
			}

			string[] parts = identityKey.Split('\t');
			if (parts.Length >= 3)
			{
				string namespaceAndClassKey = MakeNamespaceAndClassKey(parts[1], parts[2]);
				if (installedScripts.TryGetValue(MakeNamespaceAndClassLookupKey(namespaceAndClassKey), out installedGuid))
				{
					return true;
				}

				if (installedScripts.TryGetValue(MakeClassLookupKey(MakeClassKey(parts[2])), out installedGuid))
				{
					return true;
				}
			}

			installedGuid = null;
			return false;
		}

		private static void RegisterFallback(Dictionary<string, string> target, HashSet<string> ambiguousKeys, string key, string guid)
		{
			if (ambiguousKeys.Contains(key))
			{
				return;
			}

			if (target.TryGetValue(key, out string existingGuid))
			{
				if (!string.Equals(existingGuid, guid, StringComparison.OrdinalIgnoreCase))
				{
					target.Remove(key);
					ambiguousKeys.Add(key);
				}
				return;
			}

			target[key] = guid;
		}

		private static string MakeExactLookupKey(string identityKey)
		{
			return "exact\t" + identityKey;
		}

		private static string MakeNamespaceAndClassLookupKey(string namespaceAndClassKey)
		{
			return "namespace\t" + namespaceAndClassKey;
		}

		private static string MakeClassLookupKey(string classKey)
		{
			return "class\t" + classKey;
		}

		private static string MakeNamespaceAndClassKey(string namespaceName, string className)
		{
			return namespaceName + "\t" + className;
		}

		private static string MakeClassKey(string className)
		{
			return className;
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
