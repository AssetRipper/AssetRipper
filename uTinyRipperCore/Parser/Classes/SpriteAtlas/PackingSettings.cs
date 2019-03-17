using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.SpriteAtlases
{
	/// <summary>
	/// PackingParameters previously
	/// </summary>
	public struct PackingSettings : IAssetReadable, IYAMLExportable
	{
		public PackingSettings(bool _)
		{
			Padding = 2;
			BlockOffset = 1;
			AllowAlphaSplitting = false;
			EnableRotation = true;
			EnableTightPacking = true;
		}

		private static int GetSerializedVersion(Version version)
		{
			// paddingPower was renamed to padding
			if (version.IsGreaterEqual(2017, 1, 2))
			{
				return 2;
			}
			return 1;
		}

		public void Read(AssetReader reader)
		{
			Padding = reader.ReadInt32();
			BlockOffset = reader.ReadInt32();
			AllowAlphaSplitting = reader.ReadBoolean();
			EnableRotation = reader.ReadBoolean();
			EnableTightPacking = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(PaddingName, Padding);
			node.Add(BlockOffsetName, BlockOffset);
			node.Add(AllowAlphaSplittingName, AllowAlphaSplitting);
			node.Add(EnableRotationName, EnableRotation);
			node.Add(EnableTightPackingName, EnableTightPacking);
			return node;
		}

		/// <summary>
		/// PaddingPower previously
		/// </summary>
		public int Padding { get; private set; }
		public int BlockOffset { get; private set; }
		public bool AllowAlphaSplitting { get; private set; }
		public bool EnableRotation { get; private set; }
		public bool EnableTightPacking { get; private set; }

		public const string PaddingName = "padding";
		public const string BlockOffsetName = "blockOffset";
		public const string AllowAlphaSplittingName = "allowAlphaSplitting";
		public const string EnableRotationName = "enableRotation";
		public const string EnableTightPackingName = "enableTightPacking";
	}
}
