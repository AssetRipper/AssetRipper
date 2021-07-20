using AssetRipper.Converters.Project;
using AssetRipper.Parser.IO.Asset;
using AssetRipper.Parser.IO.Asset.Reader;
using AssetRipper.Parser.IO.Asset.Writer;
using AssetRipper.YAML;

namespace AssetRipper.Parser.Classes.Misc
{
	public struct Float : IAsset, IYAMLExportable
	{
		public Float(float value)
		{
			Value = value;
		}

		public static implicit operator Float(float value)
		{
			return new Float(value);
		}

		public static implicit operator float(Float value)
		{
			return value.Value;
		}

		public void Read(AssetReader reader)
		{
			Value = reader.ReadSingle();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(Value);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			return new YAMLScalarNode(Value);
		}

		public float Value { get; set; }
	}
}
