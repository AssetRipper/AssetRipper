using AssetRipper.Export.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Processing;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using System.IO;

namespace AssetRipper.Export.UnityProjects.Project;

public sealed class ProjectSettingsPostExporter : IPostExporter
{
	public void DoPostExport(GameData gameData, FullConfiguration settings, FileSystem fileSystem)
	{
		fileSystem.Directory.Create(settings.ProjectSettingsPath);

		int copiedFileCount = 0;
		foreach (ITextAsset textAsset in gameData.GameBundle.FetchAssets().OfType<ITextAsset>())
		{
			if (!TryGetProjectSettingsRelativePath(textAsset.OriginalPath, out string relativePath))
			{
				continue;
			}

			ReadOnlySpan<byte> data = textAsset.Script_C49.Data;
			if (data.Length == 0)
			{
				continue;
			}

			string outputPath = fileSystem.Path.Join(settings.ProjectSettingsPath, relativePath);
			if (fileSystem.File.Exists(outputPath))
			{
				continue;
			}

			string? directoryPath = fileSystem.Path.GetDirectoryName(outputPath);
			if (!string.IsNullOrEmpty(directoryPath))
			{
				fileSystem.Directory.Create(directoryPath);
			}
			fileSystem.File.WriteAllBytes(outputPath, data);
			copiedFileCount++;
		}

		if (copiedFileCount > 0)
		{
			Logger.Info(LogCategory.Export, $"Recovered {copiedFileCount} ProjectSettings file(s) from embedded assets.");
		}
	}

	private static bool TryGetProjectSettingsRelativePath(string? originalPath, out string relativePath)
	{
		relativePath = string.Empty;
		if (string.IsNullOrWhiteSpace(originalPath))
		{
			return false;
		}

		string normalizedPath = originalPath.Replace('\\', '/');
		int startIndex = normalizedPath.IndexOf(ProjectSettingsMarker, StringComparison.OrdinalIgnoreCase);
		if (startIndex < 0)
		{
			return false;
		}

		string candidate = normalizedPath[(startIndex + ProjectSettingsMarker.Length)..].Trim().TrimStart('/');
		if (candidate.Length == 0)
		{
			return false;
		}

		if (candidate.Contains("../", StringComparison.Ordinal)
			|| candidate.Contains("/..", StringComparison.Ordinal)
			|| candidate.IndexOf(':') >= 0)
		{
			return false;
		}

		string extension = Path.GetExtension(candidate);
		if (!AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
		{
			return false;
		}

		relativePath = candidate;
		return true;
	}

	private const string ProjectSettingsMarker = "ProjectSettings/";

	private static readonly string[] AllowedExtensions =
	[
		".asset",
		".txt",
		".json",
		".yaml",
		".yml",
	];
}
