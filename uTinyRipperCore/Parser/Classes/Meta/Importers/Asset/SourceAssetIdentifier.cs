using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AssetImporters
{
	public struct SourceAssetIdentifier : IAsset
	{
		public static bool operator ==(SourceAssetIdentifier left, SourceAssetIdentifier right)
		{
			if (left.Type != right.Type)
			{
				return false;
			}
			if (left.Assembly != right.Assembly)
			{
				return false;
			}
			if (left.Name != right.Name)
			{
				return false;
			}
			return true;
		}

		public static bool operator !=(SourceAssetIdentifier left, SourceAssetIdentifier right)
		{
			if (left.Type != right.Type)
			{
				return true;
			}
			if (left.Assembly != right.Assembly)
			{
				return true;
			}
			if (left.Name != right.Name)
			{
				return true;
			}
			return false;
		}

		public void Read(AssetReader reader)
		{
			Type = reader.ReadString();
			Assembly = reader.ReadString();
			Name = reader.ReadString();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(Type);
			writer.Write(Assembly);
			writer.Write(Name);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(TypeName, Type);
			node.Add(AssemblyName, Assembly);
			node.Add(NameName, Name);
			return node;
		}

		public override bool Equals(object obj)
		{
			if (obj is SourceAssetIdentifier asset)
			{
				return asset == this;
			}
			return false;
		}

		public override int GetHashCode()
		{
			int hash = 823;
			unchecked
			{
				hash = hash + 229 * Type.GetHashCode();
				hash = hash * 167 + Assembly.GetHashCode();
				hash = hash * 859 + Assembly.GetHashCode();
			}
			return hash;
		}

		public string Type { get; set; }
		public string Assembly { get; set; }
		public string Name { get; set; }

		public const string TypeName = "type";
		public const string AssemblyName = "assembly";
		public const string NameName = "name";
	}
}
