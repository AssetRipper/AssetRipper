using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.NewAnimationTracks
{
	public struct Channel : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			ByteOffset = reader.ReadInt32();
			Curve.Read(reader);
			AttributeName = reader.ReadString();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(ByteOffsetName, ByteOffset);
			node.Add(CurveName, Curve.ExportYAML(container));
			node.Add(AttributeNameName, AttributeName);
			return node;
		}

		public int ByteOffset { get; set; }
		public string AttributeName { get; set; }

		public const string ByteOffsetName = "byteOffset";
		public const string CurveName = "curve";
		public const string AttributeNameName = "attributeName";

		public AnimationCurveTpl<Float> Curve;
	}
}
