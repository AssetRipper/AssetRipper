using AssetRipper.Export.Configuration;
using AssetRipper.Export.Modules.Shaders.IO;
using AssetRipper.Processing;
using System.Text;

namespace AssetRipper.Export.UnityProjects.Project;

public sealed class ProjectVersionPostExporter : IPostExporter
{
	public void DoPostExport(GameData gameData, FullConfiguration settings, FileSystem fileSystem)
	{
		// Although Unity 4 and lower don't have this file, we leave it in anyway for user readibility.
		SaveProjectVersion(settings.ProjectSettingsPath, settings.Version, fileSystem);
	}

	private static void SaveProjectVersion(string projectSettingsDirectory, UnityVersion version, FileSystem fileSystem)
	{
		fileSystem.Directory.Create(projectSettingsDirectory);
		using Stream fileStream = fileSystem.File.Create(fileSystem.Path.Join(projectSettingsDirectory, "ProjectVersion.txt"));
		using StreamWriter writer = new InvariantStreamWriter(fileStream, new UTF8Encoding(false));
		writer.Write($"m_EditorVersion: {version}\n");
		if (version.Equals(5))
		{
			//Unity 5 has an extra line
			//Even on beta versions, this always seems to be zero.
			writer.Write("m_StandardAssetsVersion: 0\n");
		}

		//Beginning with 2019.1.0a10, ProjectVersion.txt files have an additional line.
		//m_EditorVersionWithRevision: 2019.4.3f1 (f880dceab6fe)
		//The revision is always 6 bytes.

		//Beginning with 2019.3.0a5, it can be acquired with the FileVersionInfo class in the System.Diagnostics namespace.
		//FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo("path/to/Unity.exe");
		//string productVersion = versionInfo.ProductVersion; //For example: 2019.4.3f1_f880dceab6fe
		//string revision = productVersion.Substring(productVersion.IndexOf('_') + 1);

		//For 2019.3.0a4 and earlier, versionInfo.ProductVersion is equal to versionInfo.FileVersion.
		//versionInfo.FileVersion is the same format for all Unity versions: 2019.4.3.16285916
		//The fourth number contains half of the revision and is a 24-bit big-endian integer.
	}
}
