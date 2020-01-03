using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Misc
{
	public struct Polygon2D : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Paths = reader.ReadAssetArrayArray<Vector2f>();
			reader.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(PathsName, Paths.ExportYAML(container));
			return node;
		}

		public Vector2f[][] Paths { get; set; }

		public const string PathsName = "m_Paths";
	}
}
