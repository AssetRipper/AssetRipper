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

		public void Read(AssetStream stream)
		{
			Value = stream.ReadSingle();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			return new YAMLScalarNode(Value);
		}

		public float Value { get; private set; }
	}
}
