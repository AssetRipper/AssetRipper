using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public class EmissionModule : ParticleSystemModule
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
		/// Less than 5.4.0
		/// </summary>
		private static bool IsBurstCountByte(Version version)
		{
			return version.IsLess(5, 4);
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

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			if (IsReadType(stream.Version))
			{
				Type = stream.ReadInt32();
			}

			RateOverTime.Read(stream);
			if (IsReadRateOverDistance(stream.Version))
			{
				RateOverDistance.Read(stream);
			}

			if (IsReadCnt(stream.Version))
			{
				Cnt0 = stream.ReadUInt16();
				Cnt1 = stream.ReadUInt16();
				Cnt2 = stream.ReadUInt16();
				Cnt3 = stream.ReadUInt16();
			}
			if (IsReadCntMax(stream.Version))
			{
				CntMax0 = stream.ReadUInt16();
				CntMax1 = stream.ReadUInt16();
				CntMax2 = stream.ReadUInt16();
				CntMax3 = stream.ReadUInt16();
			}
			if (IsReadTime(stream.Version))
			{
				Time0 = stream.ReadSingle();
				Time1 = stream.ReadSingle();
				Time2 = stream.ReadSingle();
				Time3 = stream.ReadSingle();
			}

			if (IsBurstCountByte(stream.Version))
			{
				BurstCount = stream.ReadByte();
			}
			else
			{
				BurstCount = stream.ReadInt32();
			}
			stream.AlignStream(AlignType.Align4);

			if (IsReadBursts(stream.Version))
			{
				m_bursts = stream.ReadArray<ParticleSystemEmissionBurst>();
			}
		}

		public override YAMLNode ExportYAML(IAssetsExporter exporter)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(exporter);
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("rateOverTime", RateOverTime.ExportYAML(exporter));
			node.Add("rateOverDistance", RateOverDistance.ExportYAML(exporter));
			node.Add("m_BurstCount", BurstCount);
			if (IsReadBursts(exporter.Version))
			{
				node.Add("m_Bursts", Bursts.ExportYAML(exporter));
			}
			else if (Config.IsExportTopmostSerializedVersion)
			{
				ParticleSystemEmissionBurst[] bursts = new ParticleSystemEmissionBurst[4];
				bursts[0] = new ParticleSystemEmissionBurst(Time0, Cnt0, IsReadCntMax(exporter.Version) ? CntMax0 : Cnt0);
				bursts[1] = new ParticleSystemEmissionBurst(Time1, Cnt1, IsReadCntMax(exporter.Version) ? CntMax1 : Cnt1);
				bursts[2] = new ParticleSystemEmissionBurst(Time2, Cnt2, IsReadCntMax(exporter.Version) ? CntMax2 : Cnt2);
				bursts[3] = new ParticleSystemEmissionBurst(Time3, Cnt3, IsReadCntMax(exporter.Version) ? CntMax3 : Cnt3);
				node.Add("m_Bursts", bursts.ExportYAML(exporter));
			}
			return node;
		}

		public int Type { get; private set; }
		public int BurstCount { get; private set; }
		public ushort Cnt0 { get; private set; }
		public ushort Cnt1 { get; private set; }
		public ushort Cnt2 { get; private set; }
		public ushort Cnt3 { get; private set; }
		public ushort CntMax0 { get; private set; }
		public ushort CntMax1 { get; private set; }
		public ushort CntMax2 { get; private set; }
		public ushort CntMax3 { get; private set; }
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
