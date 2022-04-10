using AssetRipper.Core.IO;
using System.IO;
using System.Text;

namespace AssetRipper.Library.Exporters
{
	public sealed class ProjectVersionPostExporter : IPostExporter
	{
		private static UnityVersion DefaultUnityVersion => new UnityVersion(2017, 3, 0, UnityVersionType.Final, 3);

		public void DoPostExport(Ripper ripper)
		{
			SaveMaxProjectVersion(ripper.Settings.ProjectSettingsPath, ripper.Settings.Version);
		}

		private static void SaveDefaultProjectVersion(string projectSettingsDirectory)
		{
			SaveProjectVersion(projectSettingsDirectory, DefaultUnityVersion);
		}

		private static void SaveMaxProjectVersion(string projectSettingsDirectory, UnityVersion exactVersion)
		{
			UnityVersion projectVersion = UnityVersion.Max(DefaultUnityVersion, exactVersion);
			SaveProjectVersion(projectSettingsDirectory, projectVersion);
		}

		private static void SaveExactProjectVersion(string projectSettingsDirectory, UnityVersion exactVersion)
		{
			SaveProjectVersion(projectSettingsDirectory, exactVersion);
		}

		private static void SaveProjectVersion(string projectSettingsDirectory, UnityVersion version)
		{
			Directory.CreateDirectory(projectSettingsDirectory);
			using Stream fileStream = System.IO.File.Create(Path.Combine(projectSettingsDirectory, "ProjectVersion.txt"));
			using StreamWriter writer = new InvariantStreamWriter(fileStream, new UTF8Encoding(false));
			writer.Write($"m_EditorVersion: {version}");
		}
	}
}
