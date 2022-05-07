using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.ShaderVariantCollection
{
	public sealed class VariantInfo : IAssetReadable, IYamlExportable
	{
		public static bool operator ==(VariantInfo left, VariantInfo right)
		{
			return left.Keywords == right.Keywords && left.PassType == right.PassType;
		}

		public static bool operator !=(VariantInfo left, VariantInfo right)
		{
			return left.Keywords != right.Keywords || left.PassType != right.PassType;
		}

		public void Read(AssetReader reader)
		{
			Keywords = reader.ReadString();
			PassType = (PassType)reader.ReadInt32();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(KeywordsName, Keywords);
			node.Add(PassTypeName, (int)PassType);
			return node;
		}

		public override int GetHashCode()
		{
			int hash = 193;
			unchecked
			{
				hash = hash + 167 * Keywords.GetHashCode();
				hash = hash * 163 + PassType.GetHashCode();
			}
			return hash;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj.GetType() != typeof(VariantInfo))
			{
				return false;
			}
			return this == (VariantInfo)obj;
		}

		public string Keywords { get; set; }
		public PassType PassType { get; set; }

		public const string KeywordsName = "keywords";
		public const string PassTypeName = "passType";
	}
}
