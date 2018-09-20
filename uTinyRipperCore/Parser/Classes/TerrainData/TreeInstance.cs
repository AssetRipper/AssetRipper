using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;

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
			node.Add("position", Position.ExportYAML(container));
			node.Add("widthScale", WidthScale);
			node.Add("heightScale", HeightScale);
			node.Add("rotation", Rotation);
			node.Add("color", Color.ExportYAML(container));
			node.Add("lightmapColor", LightmapColor.ExportYAML(container));
			node.Add("index", Index);
			return node;
		}

		public float WidthScale { get; private set; }
		public float HeightScale { get; private set; }
		public float Rotation { get; private set; }
		public int Index { get; private set; }

		public Vector3f Position;
		public ColorRGBA32 Color;
		public ColorRGBA32 LightmapColor;
	}
}
