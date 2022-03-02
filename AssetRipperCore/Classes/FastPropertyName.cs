using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.Utils;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes
{
	public sealed class FastPropertyName : IAssetReadable, IYAMLExportable
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
