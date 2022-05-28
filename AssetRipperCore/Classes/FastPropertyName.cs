using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.Utils;

using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes
{
	public sealed class FastPropertyName : IAssetReadable, IYamlExportable
	{
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		private static bool IsPlainString(UnityVersion version) => version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 1);

		public bool IsCRC28Match(uint crc)
		{
			return CrcUtils.Verify28DigestUTF8(Value, crc);
		}

		public void Read(AssetReader reader)
		{
			Value = reader.ReadString();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			if (IsPlainString(container.ExportVersion))
			{
				return new YamlScalarNode(Value);
			}
			else
			{
				YamlMappingNode node = new YamlMappingNode();
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
			if (Value == null)
			{
				return base.ToString();
			}

			return Value;
		}

		public string Value { get; set; }

		public const string NameName = "name";
	}
}
