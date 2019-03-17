using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.LightProbess;
using uTinyRipper.Classes.RenderSettingss;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class LightProbes : NamedObject
	{
		public LightProbes(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 5.0.0b1
		/// </summary>
		public static bool IsReadBakedCoefficients11(Version version)
		{
			return version.IsEqual(5, 0, 0, VersionType.Beta, 1);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadBakedLightOcclusion(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}

		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		private static bool IsReadBakedPositions(Version version)
		{
			return version.IsLess(5);
		}
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		private static bool IsReadBakedCoefficientsFirst(Version version)
		{
			return version.IsLess(5);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if(IsReadBakedPositions(reader.Version))
			{
				m_bakedPositions = reader.ReadAssetArray<Vector3f>();
			}
			if(IsReadBakedCoefficientsFirst(reader.Version))
			{
				m_bakedCoefficients = reader.ReadAssetArray<SphericalHarmonicsL2>();
			}

			Data.Read(reader);
			if (!IsReadBakedCoefficientsFirst(reader.Version))
			{
				if(IsReadBakedCoefficients11(reader.Version))
				{
					m_bakedCoefficients11 = reader.ReadAssetArray<SHCoefficientsBaked>();
				}
				else
				{
					m_bakedCoefficients = reader.ReadAssetArray<SphericalHarmonicsL2>();
				}
			}
			if(IsReadBakedLightOcclusion(reader.Version))
			{
				m_bakedLightOcclusion = reader.ReadAssetArray<LightProbeOcclusion>();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_Data", Data.ExportYAML(container));
			node.Add("m_BakedCoefficients", BakedCoefficients.ExportYAML(container));
			node.Add("m_BakedLightOcclusion", BakedLightOcclusion.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<Vector3f> BakedPositions => m_bakedPositions;
		public IReadOnlyList<SphericalHarmonicsL2> BakedCoefficients => m_bakedCoefficients;
		public IReadOnlyList<SHCoefficientsBaked> BakedCoefficients11 => m_bakedCoefficients11;
		public IReadOnlyList<LightProbeOcclusion> BakedLightOcclusion => m_bakedLightOcclusion;

		public LightProbeData Data;

		private Vector3f[] m_bakedPositions;
		private SphericalHarmonicsL2[] m_bakedCoefficients;
		private SHCoefficientsBaked[] m_bakedCoefficients11;
		private LightProbeOcclusion[] m_bakedLightOcclusion;
	}
}
