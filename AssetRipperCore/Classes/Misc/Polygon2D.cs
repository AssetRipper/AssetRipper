using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Misc
{
	public sealed class Polygon2D : IAssetReadable, IYAMLExportable
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
