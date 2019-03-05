using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class ExternalForcesModule : ParticleSystemModule
	{
		public ExternalForcesModule()
		{
		}

		public ExternalForcesModule(bool _)
		{
			Multiplier = 1.0f;
		}

		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadInfluenceFilter(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			Multiplier = reader.ReadSingle();
			if (IsReadInfluenceFilter(reader.Version))
			{
				InfluenceFilter = reader.ReadInt32();
				InfluenceMask.Read(reader);
				m_influenceList = reader.ReadArray<PPtr<ParticleSystemForceField>>();
			}
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add(MultiplierName, Multiplier);
			if (IsReadInfluenceFilter(container.Version))
			{
				node.Add(InfluenceFilterName, InfluenceFilter);
				node.Add(InfluenceMaskName, InfluenceMask.ExportYAML(container));
				node.Add(InfluenceListName, InfluenceList.ExportYAML(container));
			}
			return node;
		}

		public float Multiplier { get; private set; }
		public int InfluenceFilter { get; private set; }
		public IReadOnlyList<PPtr<ParticleSystemForceField>> InfluenceList => m_influenceList;

		public const string MultiplierName = "multiplier";
		public const string InfluenceFilterName = "influenceFilter";
		public const string InfluenceMaskName = "influenceMask";
		public const string InfluenceListName = "influenceList";		

		public BitField InfluenceMask;

		private PPtr<ParticleSystemForceField>[] m_influenceList = null;
	}
}
