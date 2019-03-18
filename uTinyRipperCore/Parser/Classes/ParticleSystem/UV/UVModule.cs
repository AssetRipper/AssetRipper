using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.ParticleSystems
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
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadTimeMode(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadStartFrame(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadSpeedRange(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
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

		/// <summary>
		/// Less than 2018.3
		/// </summary>
		private static bool IsReadFlipUFirst(Version version)
		{
			return version.IsLess(2018, 3);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadMode(reader.Version))
			{
				Mode = (ParticleSystemAnimationMode)reader.ReadInt32();
			}
			if (IsReadTimeMode(reader.Version))
			{
				TimeMode = (ParticleSystemAnimationTimeMode)reader.ReadInt32();
				FPS = reader.ReadSingle();
			}
			FrameOverTime.Read(reader);
			if (IsReadStartFrame(reader.Version))
			{
				StartFrame.Read(reader);
			}
			if (IsReadSpeedRange(reader.Version))
			{
				SpeedRange.Read(reader);
			}
			TilesX = reader.ReadInt32();
			TilesY = reader.ReadInt32();
			AnimationType = (ParticleSystemAnimationType)reader.ReadInt32();
			RowIndex = reader.ReadInt32();
			Cycles = reader.ReadSingle();
			if (IsReadUvChannelMask(reader.Version))
			{
				UvChannelMask = reader.ReadInt32();
			}
			if (IsReadFlipU(reader.Version))
			{
				if (IsReadFlipUFirst(reader.Version))
				{
					FlipU = reader.ReadSingle();
					FlipV = reader.ReadSingle();
				}
			}
			RandomRow = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);

			if (IsReadSprites(reader.Version))
			{
				m_sprites = reader.ReadAssetArray<SpriteData>();
			}
			if (IsReadFlipU(reader.Version))
			{
				if (!IsReadFlipUFirst(reader.Version))
				{
					FlipU = reader.ReadSingle();
					FlipV = reader.ReadSingle();
				}
			}
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (SpriteData spriteData in Sprites)
			{
				foreach(Object asset in spriteData.FetchDependencies(file, isLog))
				{
					yield return asset;
				}
			}
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add("mode", (int)Mode);
			if (IsReadTimeMode(container.ExportVersion))
			{
				node.Add("timeMode", (int)TimeMode);
				node.Add("fps", FPS);
			}
			node.Add("frameOverTime", FrameOverTime.ExportYAML(container));
			node.Add("startFrame", GetExportStartFrame(container.Version).ExportYAML(container));
			if (IsReadSpeedRange(container.ExportVersion))
			{
				node.Add("speedRange", SpeedRange.ExportYAML(container));
			}
			node.Add("tilesX", TilesX);
			node.Add("tilesY", TilesY);
			node.Add("animationType", (int)AnimationType);
			node.Add("rowIndex", RowIndex);
			node.Add("cycles", Cycles);
			node.Add("uvChannelMask", GetExportUvChannelMask(container.Version));
			node.Add("flipU", FlipU);
			node.Add("flipV", FlipV);
			node.Add("randomRow", RandomRow);
			node.Add("sprites", GetExportSprites(container.Version).ExportYAML(container));
			return node;
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

		public ParticleSystemAnimationMode Mode { get; private set; }
		public ParticleSystemAnimationTimeMode TimeMode { get; private set; }
		public float FPS { get; private set; }
		public int TilesX { get; private set; }
		public int TilesY { get; private set; }
		public ParticleSystemAnimationType AnimationType { get; private set; }
		public int RowIndex { get; private set; }
		public float Cycles { get; private set; }
		public int UvChannelMask { get; private set; }
		public float FlipU { get; private set; }
		public float FlipV { get; private set; }
		public bool RandomRow { get; private set; }
		public IReadOnlyList<SpriteData> Sprites => m_sprites;

		public MinMaxCurve FrameOverTime;
		public MinMaxCurve StartFrame;
		public Vector2f SpeedRange;

		private SpriteData[] m_sprites;
	}
}
