using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.ParticleSystem.Curve;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.ParticleSystem
{
	public sealed class ExternalForcesModule : ParticleSystemModule
	{
		public ExternalForcesModule() { }

		public ExternalForcesModule(bool _)
		{
			MultiplierCurve = new MinMaxCurve(1.0f);
		}

		public static int ToSerializedVersion(UnityVersion version)
		{
			// float Multiplier has been converted to MinMaxCurve multiplierCurve
			if (version.IsGreaterEqual(2019, 1, 0, UnityVersionType.Beta, 8))
			{
				return 2;
			}

			return 1;
		}

		/// <summary>
		/// 2019.1.0b8 and greater
		/// </summary>
		public static bool HasMultiplierCurve(UnityVersion version) => version.IsGreaterEqual(2019, 1, 0, UnityVersionType.Beta, 8);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasInfluenceFilter(UnityVersion version) => version.IsGreaterEqual(2018, 3);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasMultiplierCurve(reader.Version))
			{
				MultiplierCurve.Read(reader);
			}
			else
			{
				float Multiplier = reader.ReadSingle();
				MultiplierCurve = new MinMaxCurve(Multiplier);
			}

			if (HasInfluenceFilter(reader.Version))
			{
				InfluenceFilter = reader.ReadInt32();
				InfluenceMask.Read(reader);
				InfluenceList = reader.ReadAssetArray<PPtr<ParticleSystemForceField.ParticleSystemForceField>>();
			}
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			if (HasMultiplierCurve(container.ExportVersion))
			{
				node.Add(MultiplierCurveName, MultiplierCurve.ExportYAML(container));
			}
			else
			{
				node.Add(MultiplierName, Multiplier);
			}
			if (HasInfluenceFilter(container.Version))
			{
				node.Add(InfluenceFilterName, InfluenceFilter);
				node.Add(InfluenceMaskName, InfluenceMask.ExportYAML(container));
				node.Add(InfluenceListName, InfluenceList.ExportYAML(container));
			}
			return node;
		}

		public float Multiplier => MultiplierCurve.Scalar;
		public int InfluenceFilter { get; set; }
		public PPtr<ParticleSystemForceField.ParticleSystemForceField>[] InfluenceList { get; set; }

		public const string MultiplierCurveName = "multiplierCurve";
		public const string MultiplierName = "multiplier";
		public const string InfluenceFilterName = "influenceFilter";
		public const string InfluenceMaskName = "influenceMask";
		public const string InfluenceListName = "influenceList";

		public MinMaxCurve MultiplierCurve = new();
		public BitField InfluenceMask = new();
	}
}
