using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Misc
{
	public sealed class Polygon2D : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			Paths = reader.ReadAssetArrayArray<Vector2f>();
			reader.AlignStream();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(PathsName, Paths.ExportYaml(container));
			return node;
		}

		public Vector2f[][] Paths { get; set; }

		public const string PathsName = "m_Paths";
	}
}
