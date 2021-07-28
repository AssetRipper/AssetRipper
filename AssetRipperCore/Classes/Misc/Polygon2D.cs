using AssetRipper.Project;
using AssetRipper.Classes.Misc.Serializable;
using AssetRipper.IO.Asset;
using AssetRipper.IO.Extensions;
using AssetRipper.YAML;
using AssetRipper.Math;

namespace AssetRipper.Classes.Misc
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
