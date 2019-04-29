using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class ExternalForcesModule : ParticleSystemModule
	{
		public ExternalForcesModule()
		{
		}

		public ExternalForcesModule(bool _)
		{
			MultiplierCurve = new MinMaxCurve(1.0f);
		}

		/// <summary>
		/// 2019.1.0b8 and greater
		/// </summary>
		public static bool IsReadMultiplierCurve(Version version)
		{
			return version.IsGreaterEqual(2019, 1, 0, VersionType.Beta, 8);
		}
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadInfluenceFilter(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
		}

		private static int GetSerializedVersion(Version version)
		{
			// float Multiplier has been converted to MinMaxCurve multiplierCurve
			if (version.IsGreaterEqual(2019, 1, 0, VersionType.Beta, 8))
			{
				return 2;
			}

			return 1;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			if (IsReadMultiplierCurve(reader.Version))
			{
				MultiplierCurve.Read(reader);
			}
			else
			{
				float Multiplier = reader.ReadSingle();
				MultiplierCurve = new MinMaxCurve(Multiplier);
			}

			if (IsReadInfluenceFilter(reader.Version))
			{
				InfluenceFilter = reader.ReadInt32();
				InfluenceMask.Read(reader);
				m_influenceList = reader.ReadAssetArray<PPtr<ParticleSystemForceField>>();
			}
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			if (IsReadMultiplierCurve(container.ExportVersion))
			{
				node.Add(MultiplierCurveName, MultiplierCurve.ExportYAML(container));
			}
			else
			{
				node.Add(MultiplierName, Multiplier);
			}
			if (IsReadInfluenceFilter(container.Version))
			{
				node.Add(InfluenceFilterName, InfluenceFilter);
				node.Add(InfluenceMaskName, InfluenceMask.ExportYAML(container));
				node.Add(InfluenceListName, InfluenceList.ExportYAML(container));
			}
			return node;
		}

		public MinMaxCurve MultiplierCurve { get; private set; }
		public float Multiplier => MultiplierCurve.Scalar;
		public int InfluenceFilter { get; private set; }
		public IReadOnlyList<PPtr<ParticleSystemForceField>> InfluenceList => m_influenceList;

		public const string MultiplierCurveName = "multiplierCurve";
		public const string MultiplierName = "multiplier";
		public const string InfluenceFilterName = "influenceFilter";
		public const string InfluenceMaskName = "influenceMask";
		public const string InfluenceListName = "influenceList";

		public BitField InfluenceMask;

		private PPtr<ParticleSystemForceField>[] m_influenceList = null;
	}
}
