using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct UVModule : IAssetReadable, IYAMLExportable
	{
		/*private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}*/

		public void Read(AssetStream stream)
		{
			Enabled = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
			
			Mode = stream.ReadInt32();
			FrameOverTime.Read(stream);
			StartFrame.Read(stream);
			TilesX = stream.ReadInt32();
			TilesY = stream.ReadInt32();
			AnimationType = stream.ReadInt32();
			RowIndex = stream.ReadInt32();
			Cycles = stream.ReadSingle();
			UvChannelMask = stream.ReadInt32();
			FlipU = stream.ReadSingle();
			FlipV = stream.ReadSingle();
			RandomRow = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
			
			m_sprites = stream.ReadArray<SpriteData>();
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			//node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("enabled", Enabled);
			node.Add("mode", Mode);
			node.Add("frameOverTime", FrameOverTime.ExportYAML(exporter));
			node.Add("startFrame", StartFrame.ExportYAML(exporter));
			node.Add("tilesX", TilesX);
			node.Add("tilesY", TilesY);
			node.Add("animationType", AnimationType);
			node.Add("rowIndex", RowIndex);
			node.Add("cycles", Cycles);
			node.Add("uvChannelMask", UvChannelMask);
			node.Add("flipU", FlipU);
			node.Add("flipV", FlipV);
			node.Add("randomRow", RandomRow);
			node.Add("sprites", Sprites.ExportYAML(exporter));
			return node;
		}

		public bool Enabled { get; private set; }
		public int Mode { get; private set; }
		public int TilesX { get; private set; }
		public int TilesY { get; private set; }
		public int AnimationType { get; private set; }
		public int RowIndex { get; private set; }
		public float Cycles { get; private set; }
		public int UvChannelMask { get; private set; }
		public float FlipU { get; private set; }
		public float FlipV { get; private set; }
		public bool RandomRow { get; private set; }
		public IReadOnlyList<SpriteData> Sprites => m_sprites;

		public MinMaxCurve FrameOverTime;
		public MinMaxCurve StartFrame;

		private SpriteData[] m_sprites;
	}
}
