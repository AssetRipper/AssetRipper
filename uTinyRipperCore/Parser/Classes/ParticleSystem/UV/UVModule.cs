using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class UVModule : ParticleSystemModule, IDependent
	{
		public static int ToSerializedVersion(Version version)
		{
			// RandomRow has been converted to RowMode
			if (version.IsGreaterEqual(2019))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool HasMode(Version version) => version.IsGreaterEqual(2017);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasTimeMode(Version version) => version.IsGreaterEqual(2018, 3);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasStartFrame(Version version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasSpeedRange(Version version) => version.IsGreaterEqual(2018, 3);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasUvChannelMask(Version version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasFlipU(Version version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasRowMode(Version version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool HasSprites(Version version) => version.IsGreaterEqual(2017);

		/// <summary>
		/// Less than 2018.3
		/// </summary>
		private static bool HasFlipUFirst(Version version)
		{
			return version.IsLess(2018, 3);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasMode(reader.Version))
			{
				Mode = (ParticleSystemAnimationMode)reader.ReadInt32();
			}
			if (HasTimeMode(reader.Version))
			{
				TimeMode = (ParticleSystemAnimationTimeMode)reader.ReadInt32();
				FPS = reader.ReadSingle();
			}
			FrameOverTime.Read(reader);
			if (HasStartFrame(reader.Version))
			{
				StartFrame.Read(reader);
			}
			if (HasSpeedRange(reader.Version))
			{
				SpeedRange.Read(reader);
			}
			TilesX = reader.ReadInt32();
			TilesY = reader.ReadInt32();
			AnimationType = (ParticleSystemAnimationType)reader.ReadInt32();
			RowIndex = reader.ReadInt32();
			Cycles = reader.ReadSingle();
			if (HasUvChannelMask(reader.Version))
			{
				UvChannelMask = reader.ReadInt32();
			}
			if (HasFlipU(reader.Version))
			{
				if (HasFlipUFirst(reader.Version))
				{
					FlipU = reader.ReadSingle();
					FlipV = reader.ReadSingle();
				}
			}
			if (HasRowMode(reader.Version))
			{
				RowMode = (ParticleSystemAnimationRowMode)reader.ReadInt32();
			}
			else
			{
				bool RandomRow = reader.ReadBoolean();
				RowMode = RandomRow ? ParticleSystemAnimationRowMode.Random : ParticleSystemAnimationRowMode.Custom;
				reader.AlignStream();
			}

			if (HasSprites(reader.Version))
			{
				Sprites = reader.ReadAssetArray<SpriteData>();
			}
			if (HasFlipU(reader.Version))
			{
				if (!HasFlipUFirst(reader.Version))
				{
					FlipU = reader.ReadSingle();
					FlipV = reader.ReadSingle();
				}
			}
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in context.FetchDependencies(Sprites, SpritesName))
			{
				yield return asset;
			}
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(ModeName, (int)Mode);
			if (HasTimeMode(container.ExportVersion))
			{
				node.Add(TimeModeName, (int)TimeMode);
				node.Add(FpsName, FPS);
			}
			node.Add(FrameOverTimeName, FrameOverTime.ExportYAML(container));
			node.Add(StartFrameName, GetExportStartFrame(container.Version).ExportYAML(container));
			if (HasSpeedRange(container.ExportVersion))
			{
				node.Add(SpeedRangeName, SpeedRange.ExportYAML(container));
			}
			node.Add(TilesXName, TilesX);
			node.Add(TilesYName, TilesY);
			node.Add(AnimationTypeName, (int)AnimationType);
			node.Add(RowIndexName, RowIndex);
			node.Add(CyclesName, Cycles);
			node.Add(UvChannelMaskName, GetExportUvChannelMask(container.Version));
			node.Add(FlipUName, FlipU);
			node.Add(FlipVName, FlipV);
			if (HasRowMode(container.ExportVersion))
			{
				node.Add(RowModeName, (int)RowMode);
			}
			else
			{
				node.Add(RandomRowName, RandomRow);
			}
			node.Add(SpritesName, GetExportSprites(container.Version).ExportYAML(container));
			return node;
		}

		private MinMaxCurve GetExportStartFrame(Version version)
		{
			return HasStartFrame(version) ? StartFrame : new MinMaxCurve(0.0f);
		}
		private int GetExportUvChannelMask(Version version)
		{
			return HasUvChannelMask(version) ? UvChannelMask : -1;
		}
		private IReadOnlyList<SpriteData> GetExportSprites(Version version)
		{
			return HasSprites(version) ? Sprites : new SpriteData[] { default };
		}

		public ParticleSystemAnimationMode Mode { get; set; }
		public ParticleSystemAnimationTimeMode TimeMode { get; set; }
		public float FPS { get; set; }
		public int TilesX { get; set; }
		public int TilesY { get; set; }
		public ParticleSystemAnimationType AnimationType { get; set; }
		public int RowIndex { get; set; }
		public float Cycles { get; set; }
		public int UvChannelMask { get; set; }
		public float FlipU { get; set; }
		public float FlipV { get; set; }
		public bool RandomRow => RowMode == ParticleSystemAnimationRowMode.Random;
		public ParticleSystemAnimationRowMode RowMode { get; set; }
		public SpriteData[] Sprites { get; set; }

		public const string ModeName = "mode";
		public const string TimeModeName = "timeMode";
		public const string FpsName = "fps";
		public const string FrameOverTimeName = "frameOverTime";
		public const string StartFrameName = "startFrame";
		public const string SpeedRangeName = "speedRange";
		public const string TilesXName = "tilesX";
		public const string TilesYName = "tilesY";
		public const string AnimationTypeName = "animationType";
		public const string RowIndexName = "rowIndex";
		public const string CyclesName = "cycles";
		public const string UvChannelMaskName = "uvChannelMask";
		public const string FlipUName = "flipU";
		public const string FlipVName = "flipV";
		public const string RandomRowName = "randomRow";
		public const string RowModeName = "rowMode";
		public const string SpritesName = "sprites";

		public MinMaxCurve FrameOverTime;
		public MinMaxCurve StartFrame;
		public Vector2f SpeedRange;
	}
}
