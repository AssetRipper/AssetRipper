using SevenZip;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Materials
{
	public struct FastPropertyName : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		private static bool IsPlainString(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}

		public bool IsCRC28Match(uint crc)
		{
			return CRC.Verify28DigestUTF8(Value, crc);
		}

		public void Read(AssetReader reader)
		{
			Value = reader.ReadString();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			if (IsPlainString(container.ExportVersion))
			{
				return new YAMLScalarNode(Value);
			}
			else
			{
				YAMLMappingNode node = new YAMLMappingNode();
				node.Add(NameName, Value);
				return node;
			}
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public override string ToString()
		{
			if(Value == null)
			{
				return base.ToString();
			}
			return Value;
		}

		public string Value { get; private set; }

		public const string NameName = "name";
	}
}
