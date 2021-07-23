using AssetRipper.Converters.Project;
using AssetRipper.Parser.Classes.Misc;
using AssetRipper.IO.Asset;
using AssetRipper.IO.Extensions;
using AssetRipper.YAML;

namespace AssetRipper.Parser.Classes.Avatar
{
	public struct SkeletonPose : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			X = reader.ReadAssetArray<XForm>();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(XName, X == null ? YAMLSequenceNode.Empty : X.ExportYAML(container));
			return node;
		}

		public XForm[] X { get; set; }

		public const string XName = "m_X";
	}
}
