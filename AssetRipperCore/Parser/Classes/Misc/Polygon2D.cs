using AssetRipper.Converters.Project;
using AssetRipper.Parser.Classes.Misc.Serializable;
using AssetRipper.IO.Asset;
using AssetRipper.IO.Extensions;
using AssetRipper.YAML;

namespace AssetRipper.Parser.Classes.Misc
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
