using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.TerrainDatas
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

		public void Read(AssetStream stream)
		{
			Position.Read(stream);
			WidthScale = stream.ReadSingle();
			HeightScale = stream.ReadSingle();
			if (IsReadRotation(stream.Version))
			{
				Rotation = stream.ReadSingle();
			}
			Color.Read(stream);
			LightmapColor.Read(stream);
			Index = stream.ReadInt32();
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("position", Position.ExportYAML(exporter));
			node.Add("widthScale", WidthScale);
			node.Add("heightScale", HeightScale);
			node.Add("rotation", Rotation);
			node.Add("color", Color.ExportYAML(exporter));
			node.Add("lightmapColor", LightmapColor.ExportYAML(exporter));
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
