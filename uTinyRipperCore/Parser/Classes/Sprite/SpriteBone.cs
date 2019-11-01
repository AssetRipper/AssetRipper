using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Sprites
{
	public struct SpriteBone : IAsset
	{
		public void Read(AssetReader reader)
		{
			Name = reader.ReadString();
			reader.AlignStream();

			Position.Read(reader);
			Rotation.Read(reader);
			Length = reader.ReadSingle();
			ParentId = reader.ReadInt32();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(Name);
			writer.AlignStream();

			Position.Write(writer);
			Rotation.Write(writer);
			writer.Write(Length);
			writer.Write(ParentId);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(NameName, Name);
			node.Add(PositionName, Position.ExportYAML(container));
			node.Add(RotationName, Rotation.ExportYAML(container));
			node.Add(LengthName, Length);
			node.Add(ParentIdName, ParentId);
			return node;
		}

		public string Name { get; set; }
		public float Length { get; set; }
		public int ParentId { get; set; }

		public const string NameName = "name";
		public const string PositionName = "position";
		public const string RotationName = "rotation";
		public const string LengthName = "length";
		public const string ParentIdName = "parentId";

		public Vector3f Position;
		public Quaternionf Rotation;
	}
}
