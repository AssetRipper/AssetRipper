using System.Linq;
using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper;

namespace uTinyRipper.Classes.Meshes
{
	public struct MeshData : IAsset
	{
		public MeshData Convert()
		{
			MeshData instance = new MeshData();
			instance.Faces = Faces.ToArray();
			instance.Strips = Strips.ToArray();
			return instance;
		}

		public void Read(AssetReader reader)
		{
			Faces = reader.ReadAssetArray<Face>();
			Strips = reader.ReadUInt16Array();
		}

		public void Write(AssetWriter writer)
		{
			Faces.Write(writer);
			Strips.Write(writer);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(FacesName, Faces.ExportYAML(container));
			node.Add(StripsName, Strips.ExportYAML(true));
			return node;
		}

		public Face[] Faces { get; set; }
		public ushort[] Strips { get; set; }

		public const string FacesName = "faces";
		public const string StripsName = "strips";
	}
}
