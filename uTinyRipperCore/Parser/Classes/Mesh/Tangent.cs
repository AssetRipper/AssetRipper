using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Meshes
{
	public struct Tangent : IAsset
	{
		public Tangent(Vector3f normal, Vector3f tangent, float handedness)
		{
			Normal = normal;
			TangentValue = tangent;
			Handedness = handedness;
		}

		public void Read(AssetReader reader)
		{
			Normal.Read(reader);
			TangentValue.Read(reader);
			Handedness = reader.ReadSingle();
		}

		public void Write(AssetWriter writer)
		{
			Normal.Write(writer);
			TangentValue.Write(writer);
			writer.Write(Handedness);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(NormalName, Normal.ExportYAML(container));
			node.Add(TangentValueName, TangentValue.ExportYAML(container));
			node.Add(HandednessName, Handedness);
			return node;
		}

		public float Handedness { get; set; }

		public const string NormalName = "normal";
		public const string TangentValueName = "tangent";
		public const string HandednessName = "handedness";

		public Vector3f Normal;
		public Vector3f TangentValue;
	}
}
