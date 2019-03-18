using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.TerrainDatas
{
	public struct TreeInstance : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadRotation(Version version)
		{
			return version.IsGreaterEqual(5);
		}

		public void Read(AssetReader reader)
		{
			Position.Read(reader);
			WidthScale = reader.ReadSingle();
			HeightScale = reader.ReadSingle();
			if (IsReadRotation(reader.Version))
			{
				Rotation = reader.ReadSingle();
			}
			Color.Read(reader);
			LightmapColor.Read(reader);
			Index = reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(PositionName, Position.ExportYAML(container));
			node.Add(WidthScaleName, WidthScale);
			node.Add(HeightScaleName, HeightScale);
			node.Add(RotationName, Rotation);
			node.Add(ColorName, Color.ExportYAML(container));
			node.Add(LightmapColorName, LightmapColor.ExportYAML(container));
			node.Add(IndexName, Index);
			return node;
		}

		public float WidthScale { get; private set; }
		public float HeightScale { get; private set; }
		public float Rotation { get; private set; }
		public int Index { get; private set; }

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
