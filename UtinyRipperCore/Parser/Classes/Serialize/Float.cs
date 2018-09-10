using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public struct Float : IAssetReadable, IYAMLExportable
	{
		public Float(float value)
		{
			Value = value;
		}

		public static implicit operator Float(float value)
		{
			return new Float(value);
		}

		public void Read(AssetReader reader)
		{
			Value = reader.ReadSingle();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			return new YAMLScalarNode(Value);
		}

		public float Value { get; private set; }
	}
}
