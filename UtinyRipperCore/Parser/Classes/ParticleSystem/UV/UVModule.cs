using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.ParticleSystems
{
	public sealed class UVModule : ParticleSystemModule, IDependent
	{
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool IsReadMode(Version version)
		{
			return version.IsGreaterEqual(2017);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadStartFrame(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadUvChannelMask(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadFlipU(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool IsReadSprites(Version version)
		{
			return version.IsGreaterEqual(2017);
		}

		private MinMaxCurve GetExportStartFrame(Version version)
		{
			return IsReadStartFrame(version) ? StartFrame : new MinMaxCurve(0.0f);
		}
		private int GetExportUvChannelMask(Version version)
		{
			return IsReadUvChannelMask(version) ? UvChannelMask : -1;
		}
		private IReadOnlyList<SpriteData> GetExportSprites(Version version)
		{
			return IsReadSprites(version) ? Sprites : new SpriteData[] { default };
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			if (IsReadMode(stream.Version))
			{
				Mode = stream.ReadInt32();
			}
			FrameOverTime.Read(stream);
			if (IsReadStartFrame(stream.Version))
			{
				StartFrame.Read(stream);
			}
			TilesX = stream.ReadInt32();
			TilesY = stream.ReadInt32();
			AnimationType = stream.ReadInt32();
			RowIndex = stream.ReadInt32();
			Cycles = stream.ReadSingle();
			if (IsReadUvChannelMask(stream.Version))
			{
				UvChannelMask = stream.ReadInt32();
			}
			if (IsReadFlipU(stream.Version))
			{
				FlipU = stream.ReadSingle();
				FlipV = stream.ReadSingle();
			}
			RandomRow = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);

			if (IsReadSprites(stream.Version))
			{
				m_sprites = stream.ReadArray<SpriteData>();
			}
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (SpriteData spriteData in Sprites)
			{
				foreach(Object @object in spriteData.FetchDependencies(file, isLog))
				{
					yield return @object;
				}
			}
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add("mode", Mode);
			node.Add("frameOverTime", FrameOverTime.ExportYAML(container));
			node.Add("startFrame", GetExportStartFrame(container.Version).ExportYAML(container));
			node.Add("tilesX", TilesX);
			node.Add("tilesY", TilesY);
			node.Add("animationType", AnimationType);
			node.Add("rowIndex", RowIndex);
			node.Add("cycles", Cycles);
			node.Add("uvChannelMask", GetExportUvChannelMask(container.Version));
			node.Add("flipU", FlipU);
			node.Add("flipV", FlipV);
			node.Add("randomRow", RandomRow);
			node.Add("sprites", GetExportSprites(container.Version).ExportYAML(container));
			return node;
		}

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
