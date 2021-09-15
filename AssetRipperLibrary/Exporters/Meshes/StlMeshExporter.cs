using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Structure.Collections;
using AssetRipper.Library.Configuration;

namespace AssetRipper.Library.Exporters.Meshes
{
	public class StlMeshExporter : BaseMeshExporter
	{
		public StlMeshExporter(LibraryConfiguration configuration) : base(configuration)
		{
			BinaryExport = ExportFormat == MeshExportFormat.StlBinary;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Core.Classes.Object.Object asset)
		{
			return new MeshExportCollection(this, (Mesh)asset, "stl");
		}

		public override bool IsHandle(Mesh mesh)
		{
			return IsStlFormat(ExportFormat) && StlConverter.CanConvert(mesh);
		}

		public override byte[] ExportBinary(Mesh mesh)
		{
			return StlConverter.WriteBinary(mesh);
		}

		public override string ExportText(Mesh mesh)
		{
			return StlConverter.WriteString(mesh);
		}

		public static bool IsStlFormat(MeshExportFormat exportFormat)
		{
			return exportFormat == MeshExportFormat.StlAscii || exportFormat == MeshExportFormat.StlBinary;
		}
	}
}
