using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class EmissionModule : ParticleSystemModule
	{
		public static int ToSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 5))
			{
				return 4;
			}
			if (version.IsGreaterEqual(5, 5))
			{
				return 3;
			}
			if (version.IsGreaterEqual(5, 3))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		public static bool HasType(Version version) => version.IsLess(5, 5);
		/// <summary>
		/// Less or equal to 5.6.0b4
		/// </summary>
		public static bool HasCnt(Version version) => version.IsLessEqual(5, 6, 0, VersionType.Beta, 4);
		/// <summary>
		/// 5.3.0 to 5.6.0 exclusive
		/// </summary>
		public static bool HasCntMax(Version version) => version.IsGreaterEqual(5, 3) && version.IsLess(5, 6);

		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		private static bool IsIntCount(Version version) => version.IsGreaterEqual(5, 4);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasType(reader.Version))
			{
				EmissionType type = (EmissionType)reader.ReadInt32();
				if(type == EmissionType.Time)
				{
					RateOverTime.Read(reader);
					RateOverDistance = new MinMaxCurve(0.0f);
				}
				else
				{
					RateOverTime = new MinMaxCurve(0.0f);
					RateOverDistance.Read(reader);
				}
			}
			else
			{
				RateOverTime.Read(reader);
				RateOverDistance.Read(reader);
			}

			if (HasCnt(reader.Version))
			{
				int cnt0 = IsIntCount(reader.Version) ? reader.ReadInt32() : reader.ReadUInt16();
				int cnt1 = IsIntCount(reader.Version) ? reader.ReadInt32() : reader.ReadUInt16();
				int cnt2 = IsIntCount(reader.Version) ? reader.ReadInt32() : reader.ReadUInt16();
				int cnt3 = IsIntCount(reader.Version) ? reader.ReadInt32() : reader.ReadUInt16();

				int cntMax0 = cnt0;
				int cntMax1 = cnt1;
				int cntMax2 = cnt2;
				int cntMax3 = cnt3;
				if (HasCntMax(reader.Version))
				{
					cntMax0 = IsIntCount(reader.Version) ? reader.ReadInt32() : reader.ReadUInt16();
					cntMax1 = IsIntCount(reader.Version) ? reader.ReadInt32() : reader.ReadUInt16();
					cntMax2 = IsIntCount(reader.Version) ? reader.ReadInt32() : reader.ReadUInt16();
					cntMax3 = IsIntCount(reader.Version) ? reader.ReadInt32() : reader.ReadUInt16();
				}

				float time0 = reader.ReadSingle();
				float time1 = reader.ReadSingle();
				float time2 = reader.ReadSingle();
				float time3 = reader.ReadSingle();

				BurstCount = IsIntCount(reader.Version) ? reader.ReadInt32() : reader.ReadByte();
				reader.AlignStream();

				Bursts = new ParticleSystemEmissionBurst[BurstCount];
				if(BurstCount > 0)
				{
					Bursts[0] = new ParticleSystemEmissionBurst(time0, cnt0, cntMax0);
					if (BurstCount > 1)
					{
						Bursts[1] = new ParticleSystemEmissionBurst(time1, cnt1, cntMax1);
						if (BurstCount > 2)
						{
							Bursts[2] = new ParticleSystemEmissionBurst(time2, cnt2, cntMax2);
							if (BurstCount > 3)
							{
								Bursts[3] = new ParticleSystemEmissionBurst(time3, cnt3, cntMax3);
							}
						}
					}
				}
			}
			else
			{
				BurstCount = reader.ReadInt32();
				reader.AlignStream();

				Bursts = reader.ReadAssetArray<ParticleSystemEmissionBurst>();
			}
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(RateOverTimeName, RateOverTime.ExportYAML(container));
			node.Add(RateOverDistanceName, RateOverDistance.ExportYAML(container));
			node.Add(BurstCountName, BurstCount);
			node.Add(BurstsName, Bursts.ExportYAML(container));
			return node;
		}

		public int BurstCount { get; set; }
		public ParticleSystemEmissionBurst[] Bursts { get; set; }

		public const string RateOverTimeName = "rateOverTime";
		public const string RateOverDistanceName = "rateOverDistance";
		public const string BurstCountName = "m_BurstCount";
		public const string BurstsName = "m_Bursts";

		/// <summary>
		/// Rate previously
		/// </summary>
		public MinMaxCurve RateOverTime;
		public MinMaxCurve RateOverDistance;
	}
}
