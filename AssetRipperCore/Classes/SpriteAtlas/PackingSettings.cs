using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.SpriteAtlas
{
	/// <summary>
	/// PackingParameters previously
	/// </summary>
	public sealed class PackingSettings : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 2021 and greater
		/// </summary>
		public static bool HasEnableAlphaDilation(UnityVersion version) => version.IsGreaterEqual(2021);

		public PackingSettings() { }
		public PackingSettings(bool _)
		{
			Padding = 2;
			BlockOffset = 1;
			AllowAlphaSplitting = false;
			EnableRotation = true;
			EnableTightPacking = true;
			EnableAlphaDilation = true;
		}

		public static int ToSerializedVersion(UnityVersion version)
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

			if (HasEnableAlphaDilation(reader.Version))
			{
				EnableAlphaDilation = reader.ReadBoolean();
			}

			reader.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(PaddingName, Padding);
			node.Add(BlockOffsetName, BlockOffset);
			node.Add(AllowAlphaSplittingName, AllowAlphaSplitting);
			node.Add(EnableRotationName, EnableRotation);
			node.Add(EnableTightPackingName, EnableTightPacking);
			if (HasEnableAlphaDilation(container.Version))
			{
				node.Add(EnableAlphaDilationName, EnableAlphaDilation);
			}
			return node;
		}

		/// <summary>
		/// PaddingPower previously
		/// </summary>
		public int Padding { get; set; }
		public int BlockOffset { get; set; }
		public bool AllowAlphaSplitting { get; set; }
		public bool EnableRotation { get; set; }
		public bool EnableTightPacking { get; set; }
		public bool EnableAlphaDilation { get; set; }

		public const string PaddingName = "padding";
		public const string BlockOffsetName = "blockOffset";
		public const string AllowAlphaSplittingName = "allowAlphaSplitting";
		public const string EnableRotationName = "enableRotation";
		public const string EnableTightPackingName = "enableTightPacking";
		public const string EnableAlphaDilationName = "enableAlphaDilation";
	}
}
