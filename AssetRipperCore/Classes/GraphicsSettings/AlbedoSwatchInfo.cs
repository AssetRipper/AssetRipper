using AssetRipper.Core.Project;
using AssetRipper.Core.Classes.Misc.Serializable;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.YAML;
using AssetRipper.Core.Math;

namespace AssetRipper.Core.Classes.GraphicsSettings
{
	public struct AlbedoSwatchInfo : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Name = reader.ReadString();
			Color.Read(reader);
			MinLuminance = reader.ReadSingle();
			MaxLuminance = reader.ReadSingle();
			reader.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(NameName, Name);
			node.Add(ColorName, Color.ExportYAML(container));
			node.Add(MinLuminanceName, MinLuminance);
			node.Add(MaxLuminanceName, MaxLuminance);
			return node;
		}

		public string Name { get; set; }
		public float MinLuminance { get; set; }
		public float MaxLuminance { get; set; }

		public const string NameName = "name";
		public const string ColorName = "color";
		public const string MinLuminanceName = "minLuminance";
		public const string MaxLuminanceName = "maxLuminance";

		public ColorRGBAf Color;
	}
}
