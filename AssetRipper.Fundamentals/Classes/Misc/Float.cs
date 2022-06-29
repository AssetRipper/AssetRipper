using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;

using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Misc
{
	public sealed class Float : IAsset, IYamlExportable
	{
		public Float() { }

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

		public YamlNode ExportYaml(IExportContainer container)
		{
			return new YamlScalarNode(Value);
		}

		public override bool Equals(object? obj)
		{
			if (obj is Float f)
			{
				return Value == f.Value;
			}

			return false;
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public float Value { get; set; }
	}
}
