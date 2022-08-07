using AssetRipper.Core.IO;
using System.IO;
using System.Text;

namespace AssetRipper.Library.Exporters
{
	public sealed class PackageManifestPostExporter : IPostExporter
	{
		public void DoPostExport(Ripper ripper)
		{
			string packagesDirectory = Path.Combine(ripper.Settings.ProjectRootPath, "Packages");
			Directory.CreateDirectory(packagesDirectory);
			using Stream fileStream = File.Create(Path.Combine(packagesDirectory, "manifest.json"));
			using StreamWriter writer = new InvariantStreamWriter(fileStream, new UTF8Encoding(false));
			writer.Write("{\n  \"dependencies\": {}}");
		}
	}
}
