using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Misc
{
	public struct Polygon2D : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			m_paths = reader.ReadAssetArrayArray<Vector2f>();
			reader.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(PathsName, m_paths.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<IReadOnlyList<Vector2f>> Paths => m_paths;

		public const string PathsName = "m_Paths";

		private Vector2f[][] m_paths;
	}
}
