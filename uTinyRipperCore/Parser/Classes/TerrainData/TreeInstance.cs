using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.TerrainDatas
{
	public struct TreeInstance : IAsset
	{
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasRotation(Version version) => version.IsGreaterEqual(5);

		public void Read(AssetReader reader)
		{
			Position.Read(reader);
			WidthScale = reader.ReadSingle();
			HeightScale = reader.ReadSingle();
			if (HasRotation(reader.Version))
			{
				Rotation = reader.ReadSingle();
			}
			Color.Read(reader);
			LightmapColor.Read(reader);
			Index = reader.ReadInt32();
		}

		public void Write(AssetWriter writer)
		{
			Position.Write(writer);
			writer.Write(WidthScale);
			writer.Write(HeightScale);
			if (HasRotation(writer.Version))
			{
				writer.Write(Rotation);
			}
			Color.Write(writer);
			LightmapColor.Write(writer);
			writer.Write(Index);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(PositionName, Position.ExportYAML(container));
			node.Add(WidthScaleName, WidthScale);
			node.Add(HeightScaleName, HeightScale);
			if (HasRotation(container.ExportVersion))
			{
				node.Add(RotationName, Rotation);
			}
			node.Add(ColorName, Color.ExportYAML(container));
			node.Add(LightmapColorName, LightmapColor.ExportYAML(container));
			node.Add(IndexName, Index);
			return node;
		}

		public float WidthScale { get; set; }
		public float HeightScale { get; set; }
		public float Rotation { get; set; }
		public int Index { get; set; }

		public const string PositionName = "position";
		public const string WidthScaleName = "widthScale";
		public const string HeightScaleName = "heightScale";
		public const string RotationName = "rotation";
		public const string ColorName = "color";
		public const string LightmapColorName = "lightmapColor";
		public const string IndexName = "index";

		public Vector3f Position;
		public ColorRGBA32 Color;
		public ColorRGBA32 LightmapColor;
	}
}
