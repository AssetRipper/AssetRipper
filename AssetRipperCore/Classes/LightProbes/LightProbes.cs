using AssetRipper.Core.Classes.RenderSettings;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.LightProbes
{
	public sealed class LightProbes : NamedObject
	{
		public LightProbes(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// 5.0.0b1
		/// </summary>
		public static bool HasBakedCoefficients11(UnityVersion version) => version.IsEqual(5, 0, 0, UnityVersionType.Beta, 1);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasBakedLightOcclusion(UnityVersion version) => version.IsGreaterEqual(5, 4);

		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		private static bool HasBakedPositions(UnityVersion version)
		{
			return version.IsLess(5);
		}
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		private static bool HasBakedCoefficientsFirst(UnityVersion version)
		{
			return version.IsLess(5);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasBakedPositions(reader.Version))
			{
				BakedPositions = reader.ReadAssetArray<Vector3f>();
			}
			if (HasBakedCoefficientsFirst(reader.Version))
			{
				BakedCoefficients = reader.ReadAssetArray<SphericalHarmonicsL2>();
			}

			Data.Read(reader);
			if (!HasBakedCoefficientsFirst(reader.Version))
			{
				if (HasBakedCoefficients11(reader.Version))
				{
					BakedCoefficients11 = reader.ReadAssetArray<SHCoefficientsBaked>();
				}
				else
				{
					BakedCoefficients = reader.ReadAssetArray<SphericalHarmonicsL2>();
				}
			}
			if (HasBakedLightOcclusion(reader.Version))
			{
				BakedLightOcclusion = reader.ReadAssetArray<LightProbeOcclusion>();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(DataName, Data.ExportYAML(container));
			node.Add(BakedCoefficientsName, BakedCoefficients.ExportYAML(container));
			if (HasBakedLightOcclusion(container.Version))
			{
				node.Add(BakedLightOcclusionName, BakedLightOcclusion.ExportYAML(container));
			}

			return node;
		}

		public Vector3f[] BakedPositions { get; set; }
		public SphericalHarmonicsL2[] BakedCoefficients { get; set; }
		public SHCoefficientsBaked[] BakedCoefficients11 { get; set; }
		public LightProbeOcclusion[] BakedLightOcclusion { get; set; }

		public const string DataName = "m_Data";
		public const string BakedCoefficientsName = "m_BakedCoefficients";
		public const string BakedLightOcclusionName = "m_BakedLightOcclusion";

		public LightProbeData Data = new();
	}
}
