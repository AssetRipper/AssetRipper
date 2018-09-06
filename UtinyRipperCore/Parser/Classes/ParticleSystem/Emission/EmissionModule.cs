using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
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
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadRateOverDistance(Version version)
		{
			return version.IsGreaterEqual(5, 5);
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
		/// Less than 5.6.0
		/// </summary>
		public static bool IsReadTime(Version version)
		{
			return version.IsLess(5, 6);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadBursts(Version version)
		{
			return version.IsGreaterEqual(5, 6);
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
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 4;
			}
			
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

		private MinMaxCurve GetExportRateOverDistance(Version version)
		{
			return IsReadRateOverDistance(version) ? RateOverDistance : new MinMaxCurve(0.0f);
		}
		private IReadOnlyList<ParticleSystemEmissionBurst> GetExportBursts(Version version)
		{
			if (IsReadBursts(version))
			{
				return Bursts;
			}
			else
			{
				ParticleSystemEmissionBurst[] bursts = new ParticleSystemEmissionBurst[4];
				bursts[0] = new ParticleSystemEmissionBurst(Time0, Cnt0, IsReadCntMax(version) ? CntMax0 : Cnt0);
				bursts[1] = new ParticleSystemEmissionBurst(Time1, Cnt1, IsReadCntMax(version) ? CntMax1 : Cnt1);
				bursts[2] = new ParticleSystemEmissionBurst(Time2, Cnt2, IsReadCntMax(version) ? CntMax2 : Cnt2);
				bursts[3] = new ParticleSystemEmissionBurst(Time3, Cnt3, IsReadCntMax(version) ? CntMax3 : Cnt3);
				return bursts;
			}
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadType(reader.Version))
			{
				Type = reader.ReadInt32();
			}

			RateOverTime.Read(reader);
			if (IsReadRateOverDistance(reader.Version))
			{
				RateOverDistance.Read(reader);
			}

			if (IsReadCnt(reader.Version))
			{
				Cnt0 = IsIntCount(reader.Version) ? reader.ReadInt32() : reader.ReadUInt16();
				Cnt1 = IsIntCount(reader.Version) ? reader.ReadInt32() : reader.ReadUInt16();
				Cnt2 = IsIntCount(reader.Version) ? reader.ReadInt32() : reader.ReadUInt16();
				Cnt3 = IsIntCount(reader.Version) ? reader.ReadInt32() : reader.ReadUInt16();
			}
			if (IsReadCntMax(reader.Version))
			{
				CntMax0 = IsIntCount(reader.Version) ? reader.ReadInt32() : reader.ReadUInt16();
				CntMax1 = IsIntCount(reader.Version) ? reader.ReadInt32() : reader.ReadUInt16();
				CntMax2 = IsIntCount(reader.Version) ? reader.ReadInt32() : reader.ReadUInt16();
				CntMax3 = IsIntCount(reader.Version) ? reader.ReadInt32() : reader.ReadUInt16();
			}
			if (IsReadTime(reader.Version))
			{
				Time0 = reader.ReadSingle();
				Time1 = reader.ReadSingle();
				Time2 = reader.ReadSingle();
				Time3 = reader.ReadSingle();
			}

			if (IsIntCount(reader.Version))
			{
				BurstCount = reader.ReadInt32();
			}
			else
			{
				BurstCount = reader.ReadByte();
			}
			reader.AlignStream(AlignType.Align4);

			if (IsReadBursts(reader.Version))
			{
				m_bursts = reader.ReadArray<ParticleSystemEmissionBurst>();
			}
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("rateOverTime", RateOverTime.ExportYAML(container));
			node.Add("rateOverDistance", GetExportRateOverDistance(container.Version).ExportYAML(container));
			node.Add("m_BurstCount", BurstCount);
			node.Add("m_Bursts", GetExportBursts(container.Version).ExportYAML(container));
			return node;
		}

		public int Type { get; private set; }
		public int BurstCount { get; private set; }
		public int Cnt0 { get; private set; }
		public int Cnt1 { get; private set; }
		public int Cnt2 { get; private set; }
		public int Cnt3 { get; private set; }
		public int CntMax0 { get; private set; }
		public int CntMax1 { get; private set; }
		public int CntMax2 { get; private set; }
		public int CntMax3 { get; private set; }
		public float Time0 { get; private set; }
		public float Time1 { get; private set; }
		public float Time2 { get; private set; }
		public float Time3 { get; private set; }
		public IReadOnlyList<ParticleSystemEmissionBurst> Bursts => m_bursts;

		/// <summary>
		/// Rate previously
		/// </summary>
		public MinMaxCurve RateOverTime;
		public MinMaxCurve RateOverDistance;

		private ParticleSystemEmissionBurst[] m_bursts;
	}
}
