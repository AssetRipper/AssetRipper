using AssetRipper.Core.IO;
using System.IO;
using System.Text;

namespace AssetRipper.Library.Exporters
{
	public sealed class ProjectVersionPostExporter : IPostExporter
	{
		public void DoPostExport(Ripper ripper)
		{
			SaveProjectVersion(ripper.Settings.ProjectSettingsPath, ripper.Settings.Version);
		}

		private static void SaveProjectVersion(string projectSettingsDirectory, UnityVersion version)
		{
			Directory.CreateDirectory(projectSettingsDirectory);
			using Stream fileStream = File.Create(Path.Combine(projectSettingsDirectory, "ProjectVersion.txt"));
			using StreamWriter writer = new InvariantStreamWriter(fileStream, new UTF8Encoding(false));
			writer.Write($"m_EditorVersion: {version}");
		}
	}
}
