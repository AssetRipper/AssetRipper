using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes
{
	public struct PropertyName : IAsset
	{
		public static bool operator ==(PropertyName lhs, PropertyName rhs)
		{
			return lhs.ID == rhs.ID;
		}

		public static bool operator !=(PropertyName lhs, PropertyName rhs)
		{
			return lhs.ID != rhs.ID;
		}

		public void Read(AssetReader reader)
		{
			ID = reader.ReadInt32();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(ID);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			return new YAMLScalarNode(ID == 0 ? string.Empty : $"Unknown_{unchecked((uint)ID)}");
		}

		public override int GetHashCode()
		{
			return ID;
		}

		public override bool Equals(object other)
		{
			if (other is PropertyName propertyName)
			{
				return propertyName == this;
			}
			return false;
		}

		public int ID { get; set; }
	}
}
