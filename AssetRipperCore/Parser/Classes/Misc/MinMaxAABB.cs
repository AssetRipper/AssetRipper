using AssetRipper.Converters.Project;
using AssetRipper.Parser.Classes.Misc.Serializable;
using AssetRipper.Parser.IO.Asset;
using AssetRipper.Parser.IO.Asset.Reader;
using AssetRipper.Parser.IO.Asset.Writer;
using AssetRipper.YAML;

namespace AssetRipper.Parser.Classes.Misc
{
	public struct MinMaxAABB : IAsset
	{
		public void Read(AssetReader reader)
		{
			Min.Read(reader);
			Max.Read(reader);
		}

		public void Write(AssetWriter writer)
		{
			Min.Write(writer);
			Max.Write(writer);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(MinName, Min.ExportYAML(container));
			node.Add(MaxName, Max.ExportYAML(container));
			return node;
		}

		public const string MinName = "m_Min";
		public const string MaxName = "m_Max";

		public Vector3f Min;
		public Vector3f Max;
	}
}
