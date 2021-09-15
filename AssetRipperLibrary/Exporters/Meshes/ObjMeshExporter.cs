using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Structure.Collections;
using AssetRipper.Library.Configuration;

namespace AssetRipper.Library.Exporters.Meshes
{
	public class ObjMeshExporter : BaseMeshExporter
	{
		public ObjMeshExporter(LibraryConfiguration configuration) : base(configuration)
		{
			BinaryExport = false;
		}

		public override bool IsHandle(Mesh mesh)
		{
			return ExportFormat == MeshExportFormat.Obj && ObjConverter.CanConvert(mesh);
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Core.Classes.Object.Object asset)
		{
			return new MeshExportCollection(this, (Mesh)asset, "obj");
		}

		public override string ExportText(Mesh mesh)
		{
			return ObjConverter.ConvertToObjString(mesh, true);
		}
	}
}
