using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class EmissionModule : ParticleSystemModule
	{
		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		public static bool IsReadType(Version version)
		{
			return version.IsLess(5, 5);
		}
		/// <summary>
		/// Less than 5.6.0
		/// </summary>
		public static bool IsReadCnt(Version version)
		{
			return version.IsLess(5, 6);
		}
		/// <summary>
		/// 5.3.0 to 5.6.0 exclusive
		/// </summary>
		public static bool IsReadCntMax(Version version)
		{
			return version.IsGreaterEqual(5, 3) && version.IsLess(5, 6);
		}

		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		private static bool IsIntCount(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(5, 6))
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

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadType(reader.Version))
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

			if (IsReadCnt(reader.Version))
			{
				int cnt0 = IsIntCount(reader.Version) ? reader.ReadInt32() : reader.ReadUInt16();
				int cnt1 = IsIntCount(reader.Version) ? reader.ReadInt32() : reader.ReadUInt16();
				int cnt2 = IsIntCount(reader.Version) ? reader.ReadInt32() : reader.ReadUInt16();
				int cnt3 = IsIntCount(reader.Version) ? reader.ReadInt32() : reader.ReadUInt16();

				int cntMax0 = cnt0;
				int cntMax1 = cnt1;
				int cntMax2 = cnt2;
				int cntMax3 = cnt3;
				if (IsReadCntMax(reader.Version))
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
				reader.AlignStream(AlignType.Align4);

				m_bursts = new ParticleSystemEmissionBurst[BurstCount];
				if(BurstCount > 0)
				{
					m_bursts[0] = new ParticleSystemEmissionBurst(time0, cnt0, cntMax0);
					if (BurstCount > 1)
					{
						m_bursts[1] = new ParticleSystemEmissionBurst(time1, cnt1, cntMax1);
						if (BurstCount > 2)
						{
							m_bursts[2] = new ParticleSystemEmissionBurst(time2, cnt2, cntMax2);
							if (BurstCount > 3)
							{
								m_bursts[3] = new ParticleSystemEmissionBurst(time3, cnt3, cntMax3);
							}
						}
					}
				}
			}
			else
			{
				BurstCount = reader.ReadInt32();
				reader.AlignStream(AlignType.Align4);

				m_bursts = reader.ReadAssetArray<ParticleSystemEmissionBurst>();
			}
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(RateOverTimeName, RateOverTime.ExportYAML(container));
			node.Add(RateOverDistanceName, RateOverDistance.ExportYAML(container));
			node.Add(BurstCountName, BurstCount);
			node.Add(BurstsName, Bursts.ExportYAML(container));
			return node;
		}

		public int BurstCount { get; private set; }
		public IReadOnlyList<ParticleSystemEmissionBurst> Bursts => m_bursts;

		public const string RateOverTimeName = "rateOverTime";
		public const string RateOverDistanceName = "rateOverDistance";
		public const string BurstCountName = "m_BurstCount";
		public const string BurstsName = "m_Bursts";

		/// <summary>
		/// Rate previously
		/// </summary>
		public MinMaxCurve RateOverTime;
		public MinMaxCurve RateOverDistance;

		private ParticleSystemEmissionBurst[] m_bursts;
	}
}
