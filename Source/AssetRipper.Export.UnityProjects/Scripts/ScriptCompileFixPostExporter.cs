using AssetRipper.Export.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Processing;
using System.IO;
using System.Text.RegularExpressions;

namespace AssetRipper.Export.UnityProjects.Scripts;

public sealed class ScriptCompileFixPostExporter : IPostExporter
{
	private const string JobReflectionFilePrefix = "__JobReflectionRegistrationOutput__";
	private static readonly Regex NonSerializedAttributeLineRegex = new(@"^\[(?:field:\s*)?(?:(?:global::)?System\.)?NonSerialized(?:Attribute)?\]\s*$", RegexOptions.Compiled);
	private static readonly Regex FieldOffsetAttributeLineRegex = new(@"^\[(?:field:\s*)?(?:(?:global::)?System\.Runtime\.InteropServices\.)?FieldOffset(?:Attribute)?\s*\(.*\)\]\s*$", RegexOptions.Compiled);
	private static readonly Regex TypeDeclarationRegex = new(@"\b(?:class|struct)\b", RegexOptions.Compiled);
	private static readonly Regex LogTypeReferenceRegex = new(@"(?<![\w\.])LogType\.", RegexOptions.Compiled);

	public void DoPostExport(GameData gameData, FullConfiguration settings, FileSystem fileSystem)
	{
		if (!settings.Version.GreaterThanOrEquals(2019, 1))
		{
			return;
		}

		string scriptsPath = fileSystem.Path.Join(settings.AssetsPath, "Scripts");
		if (!fileSystem.Directory.Exists(scriptsPath))
		{
			return;
		}

		int rewrittenFiles = 0;
		int removedFiles = 0;

		foreach (string filePath in fileSystem.Directory.EnumerateFiles(scriptsPath, "*.cs", SearchOption.AllDirectories).ToArray())
		{
			if (ShouldDeleteStaleGeneratedFile(filePath, fileSystem))
			{
				DeleteFileWithMeta(filePath, fileSystem);
				removedFiles++;
				continue;
			}

			string originalText = fileSystem.File.ReadAllText(filePath);
			string updatedText = FixSource(originalText, settings.ExportSettings);
			if (string.Equals(originalText, updatedText, StringComparison.Ordinal))
			{
				continue;
			}

			fileSystem.File.WriteAllText(filePath, updatedText);
			rewrittenFiles++;
		}

		if (rewrittenFiles > 0 || removedFiles > 0)
		{
			Logger.Info(LogCategory.Export, $"Applied exported script compile fixes to {rewrittenFiles} files and removed {removedFiles} stale generated files.");
		}
	}

	private static bool ShouldDeleteStaleGeneratedFile(string filePath, FileSystem fileSystem)
	{
		string fileName = fileSystem.Path.GetFileName(filePath);
		return fileName.StartsWith(JobReflectionFilePrefix, StringComparison.Ordinal);
	}

	private static void DeleteFileWithMeta(string filePath, FileSystem fileSystem)
	{
		if (fileSystem.File.Exists(filePath))
		{
			fileSystem.File.Delete(filePath);
		}

		string metaPath = $"{filePath}.meta";
		if (fileSystem.File.Exists(metaPath))
		{
			fileSystem.File.Delete(metaPath);
		}
	}

	private static string FixSource(string text, ExportSettings settings)
	{
		bool hadCrLf = text.Contains("\r\n", StringComparison.Ordinal);
		string updatedText = text.Replace("\r\n", "\n", StringComparison.Ordinal);
		updatedText = CollapseDuplicateNonSerializedAttributes(updatedText);
		if (settings.EnableAmbiguousLogFix)
		{
			updatedText = QualifyAmbiguousFusionLogType(updatedText);
		}
		updatedText = FixBrokenExplicitLayout(updatedText);
		if (settings.EnableSimulationAccessFix)
		{
			updatedText = FixFusionSimulationAccess(updatedText);
		}
		if (hadCrLf)
		{
			updatedText = updatedText.Replace("\n", "\r\n", StringComparison.Ordinal);
		}
		return updatedText;
	}

	private static string FixFusionSimulationAccess(string text)
	{
		if (!text.Contains("using Fusion;", StringComparison.Ordinal))
		{
			return text;
		}

		// Replace Runner.Simulation with Runner._simulation to resolve access issues in decompiled Fusion scripts
		return text.Replace(".Runner.Simulation", ".Runner._simulation", StringComparison.Ordinal)
				   .Replace("runner.Simulation", "runner._simulation", StringComparison.Ordinal);
	}

	private static string CollapseDuplicateNonSerializedAttributes(string text)
	{
		string[] lines = text.Split('\n');
		List<string> output = new(lines.Length);
		bool seenNonSerializedInAttributeBlock = false;

		foreach (string line in lines)
		{
			string trimmedLine = line.Trim();
			if (trimmedLine.Length == 0)
			{
				seenNonSerializedInAttributeBlock = false;
				output.Add(line);
				continue;
			}

			if (trimmedLine.StartsWith("[", StringComparison.Ordinal))
			{
				if (NonSerializedAttributeLineRegex.IsMatch(trimmedLine))
				{
					if (seenNonSerializedInAttributeBlock)
					{
						continue;
					}

					seenNonSerializedInAttributeBlock = true;
				}

				output.Add(line);
				continue;
			}

			seenNonSerializedInAttributeBlock = false;
			output.Add(line);
		}

		return string.Join("\n", output);
	}

	private static string QualifyAmbiguousFusionLogType(string text)
	{
		if (!text.Contains("using Fusion;", StringComparison.Ordinal)
			|| !text.Contains("using UnityEngine;", StringComparison.Ordinal)
			|| text.IndexOf("LogType.", StringComparison.Ordinal) < 0)
		{
			return text;
		}

		return LogTypeReferenceRegex.Replace(text, "UnityEngine.LogType.");
	}

	private static string FixBrokenExplicitLayout(string text)
	{
		string[] lines = text.Split('\n');
		bool changed = false;

		for (int i = 0; i < lines.Length; i++)
		{
			if (lines[i].IndexOf("StructLayout(LayoutKind.Explicit", StringComparison.Ordinal) < 0)
			{
				continue;
			}

			int typeLineIndex = FindTypeDeclarationLine(lines, i);
			if (typeLineIndex < 0)
			{
				continue;
			}

			int blockStartLineIndex = FindBlockStartLine(lines, typeLineIndex);
			if (blockStartLineIndex < 0)
			{
				continue;
			}

			int blockEndLineIndex = FindBlockEndLine(lines, blockStartLineIndex);
			if (blockEndLineIndex < 0 || !HasMissingFieldOffsets(lines, blockStartLineIndex, blockEndLineIndex))
			{
				i = Math.Max(i, blockEndLineIndex);
				continue;
			}

			for (int j = i; j <= typeLineIndex; j++)
			{
				if (lines[j].IndexOf("LayoutKind.Explicit", StringComparison.Ordinal) >= 0)
				{
					lines[j] = lines[j].Replace("LayoutKind.Explicit", "LayoutKind.Sequential", StringComparison.Ordinal);
					changed = true;
				}
			}

			for (int j = blockStartLineIndex + 1; j < blockEndLineIndex; j++)
			{
				if (FieldOffsetAttributeLineRegex.IsMatch(lines[j].Trim()))
				{
					lines[j] = string.Empty;
					changed = true;
				}
			}

			i = blockEndLineIndex;
		}

		return changed ? string.Join("\n", lines) : text;
	}

	private static int FindTypeDeclarationLine(IReadOnlyList<string> lines, int startIndex)
	{
		for (int i = startIndex; i < lines.Count; i++)
		{
			string trimmedLine = lines[i].Trim();
			if (trimmedLine.Length == 0 || trimmedLine.StartsWith("[", StringComparison.Ordinal))
			{
				continue;
			}

			return TypeDeclarationRegex.IsMatch(trimmedLine) ? i : -1;
		}

		return -1;
	}

	private static int FindBlockStartLine(IReadOnlyList<string> lines, int typeLineIndex)
	{
		for (int i = typeLineIndex; i < lines.Count; i++)
		{
			if (lines[i].IndexOf('{', StringComparison.Ordinal) >= 0)
			{
				return i;
			}
		}

		return -1;
	}

	private static int FindBlockEndLine(IReadOnlyList<string> lines, int blockStartLineIndex)
	{
		int depth = 0;
		for (int i = blockStartLineIndex; i < lines.Count; i++)
		{
			string line = lines[i];
			depth += line.Count(static c => c == '{');
			depth -= line.Count(static c => c == '}');
			if (depth == 0)
			{
				return i;
			}
		}

		return -1;
	}

	private static bool HasMissingFieldOffsets(IReadOnlyList<string> lines, int blockStartLineIndex, int blockEndLineIndex)
	{
		bool seenFieldOffsetInAttributeBlock = false;

		for (int i = blockStartLineIndex + 1; i < blockEndLineIndex; i++)
		{
			string trimmedLine = lines[i].Trim();
			if (trimmedLine.Length == 0)
			{
				seenFieldOffsetInAttributeBlock = false;
				continue;
			}

			if (trimmedLine.StartsWith("[", StringComparison.Ordinal))
			{
				if (FieldOffsetAttributeLineRegex.IsMatch(trimmedLine))
				{
					seenFieldOffsetInAttributeBlock = true;
				}
				continue;
			}

			if (IsInstanceFieldDeclaration(trimmedLine))
			{
				if (!seenFieldOffsetInAttributeBlock)
				{
					return true;
				}
			}

			seenFieldOffsetInAttributeBlock = false;
		}

		return false;
	}

	private static bool IsInstanceFieldDeclaration(string trimmedLine)
	{
		if (!trimmedLine.EndsWith(";", StringComparison.Ordinal)
			|| trimmedLine.IndexOf("{", StringComparison.Ordinal) >= 0
			|| trimmedLine.IndexOf("(", StringComparison.Ordinal) >= 0
			|| trimmedLine.Contains("=>", StringComparison.Ordinal))
		{
			return false;
		}

		if (TypeDeclarationRegex.IsMatch(trimmedLine)
			|| ContainsWord(trimmedLine, "delegate")
			|| ContainsWord(trimmedLine, "event")
			|| ContainsWord(trimmedLine, "const")
			|| ContainsWord(trimmedLine, "static"))
		{
			return false;
		}

		return true;
	}

	private static bool ContainsWord(string text, string word)
	{
		int index = text.IndexOf(word, StringComparison.Ordinal);
		while (index >= 0)
		{
			bool startBoundary = index == 0 || !char.IsLetterOrDigit(text[index - 1]) && text[index - 1] != '_';
			int endIndex = index + word.Length;
			bool endBoundary = endIndex >= text.Length || !char.IsLetterOrDigit(text[endIndex]) && text[endIndex] != '_';
			if (startBoundary && endBoundary)
			{
				return true;
			}

			index = text.IndexOf(word, index + word.Length, StringComparison.Ordinal);
		}

		return false;
	}
}
