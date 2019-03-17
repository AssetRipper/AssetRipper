using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
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

		public static Float DefaultWeight => 1.0f / 3.0f;

		public float Value { get; private set; }
	}
}
