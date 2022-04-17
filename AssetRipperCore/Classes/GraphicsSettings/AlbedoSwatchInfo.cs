using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.GraphicsSettings
{
	public sealed class AlbedoSwatchInfo : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			Name = reader.ReadString();
			Color.Read(reader);
			MinLuminance = reader.ReadSingle();
			MaxLuminance = reader.ReadSingle();
			reader.AlignStream();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(NameName, Name);
			node.Add(ColorName, Color.ExportYaml(container));
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

		public ColorRGBAf Color = new();
	}
}
