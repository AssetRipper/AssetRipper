using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Sprite
{
	public sealed class SpriteBone : IAsset
	{
		/// <summary>
		/// 2021 and greater
		/// </summary>
		public static bool HasGuid(UnityVersion version) => version.IsGreaterEqual(2021);
		/// <summary>
		/// 2021 and greater
		/// </summary>
		public static bool HasColor(UnityVersion version) => version.IsGreaterEqual(2021);

		public void Read(AssetReader reader)
		{
			Name = reader.ReadString();
			reader.AlignStream();

			if (HasGuid(reader.Version))
			{
				Guid = reader.ReadString();
			}

			Position.Read(reader);
			Rotation.Read(reader);
			Length = reader.ReadSingle();
			ParentId = reader.ReadInt32();
			if (HasColor(reader.Version))
			{
				Color.Read(reader);
			}
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(Name);
			writer.AlignStream();

			if (HasGuid(writer.Version))
			{
				writer.Write(Guid);
			}

			Position.Write(writer);
			Rotation.Write(writer);
			writer.Write(Length);
			writer.Write(ParentId);

			if (HasColor(writer.Version))
			{
				Color.Write(writer);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(NameName, Name);
			if (HasGuid(container.ExportVersion))
			{
				node.Add(GuidName, Guid);
			}
			node.Add(PositionName, Position.ExportYAML(container));
			node.Add(RotationName, Rotation.ExportYAML(container));
			node.Add(LengthName, Length);
			node.Add(ParentIdName, ParentId);
			if (HasColor(container.ExportVersion))
			{
				node.Add(ColorName, Color.ExportYAML(container));
			}
			return node;
		}

		public string Name { get; set; }
		public string Guid { get; set; }
		public float Length { get; set; }
		public int ParentId { get; set; }
		public ColorRGBA32 Color { get; set; }

		public const string NameName = "name";
		public const string GuidName = "guid";
		public const string PositionName = "position";
		public const string RotationName = "rotation";
		public const string LengthName = "length";
		public const string ParentIdName = "parentId";
		public const string ColorName = "color";

		public Vector3f Position = new();
		public Quaternionf Rotation = new();
	}
}
