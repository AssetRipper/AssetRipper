using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Avatar
{
	public sealed class SkeletonPose : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			X = reader.ReadAssetArray<XForm>();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(XName, X == null ? YamlSequenceNode.Empty : X.ExportYaml(container));
			return node;
		}

		public XForm[] X { get; set; }

		public const string XName = "m_X";
	}
}
