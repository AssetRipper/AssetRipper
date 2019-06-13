using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Lights
{
	public struct ShadowSettings : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadCustomResolution(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// Less than 3.4.0
		/// </summary>
		public static bool IsReadProjection(Version version)
		{
			return version.IsLess(3, 4);
		}
		/// <summary>
		/// Less than 3
		/// </summary>
		public static bool IsReadConstantBias(Version version)
		{
			return version.IsLess(3);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadBias(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 3.2.0 to 5.0.0beta
		/// </summary>
		public static bool IsReadSoftness(Version version)
		{
			return version.IsGreaterEqual(3, 2) && version.IsLess(5, 0, 0, VersionType.Beta);
		}
		/// <summary>
		/// 5.0.0f and greater
		/// </summary>
		public static bool IsReadNormalBias(Version version)
		{
			return version.IsGreater(5, 0, 0, VersionType.Beta);
		}
		/// <summary>
		/// 5.3.0b6 and greater
		/// </summary>
		public static bool IsReadNearPlane(Version version)
		{
			return version.IsGreaterEqual(5, 3, 0, VersionType.Beta, 6);
		}
		/// <summary>
		/// 2019.1.0b4 and greater
		/// </summary>
		public static bool IsReadCullingMatrixOverride(Version version)
		{
			return version.IsGreaterEqual(2019, 1, 0, VersionType.Beta, 4);
		}

		public void Read(AssetReader reader)
		{
			Type = (LightShadows)reader.ReadInt32();
			Resolution = reader.ReadInt32();
			if (IsReadCustomResolution(reader.Version))
			{
				CustomResolution = reader.ReadInt32();
			}
			Strength = reader.ReadSingle();
			if (IsReadProjection(reader.Version))
			{
				Projection = reader.ReadInt32();
			}
			if (IsReadConstantBias(reader.Version))
			{
				ConstantBias = reader.ReadSingle();
				ObjectSizeBias = reader.ReadSingle();
			}
			if (IsReadBias(reader.Version))
			{
				Bias = reader.ReadSingle();
			}
			if (IsReadSoftness(reader.Version))
			{
				Softness = reader.ReadSingle();
				SoftnessFade = reader.ReadSingle();
			}
			if (IsReadNormalBias(reader.Version))
			{
				NormalBias = reader.ReadSingle();
			}
			if (IsReadNearPlane(reader.Version))
			{
				NearPlane = reader.ReadSingle();
			}
			if (IsReadCullingMatrixOverride(reader.Version))
			{
				CullingMatrixOverride.Read(reader);
				UseCullingMatrixOverride = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(TypeName, (int)Type);
			node.Add(ResolutionName, Resolution);
			node.Add(CustomResolutionName, CustomResolution);
			node.Add(StrengthName, Strength);
			node.Add(BiasName, Bias);
			node.Add(NormalBiasName, NormalBias);
			node.Add(NearPlaneName, NearPlane);
			if (IsReadCullingMatrixOverride(container.ExportVersion))
			{
				node.Add(CullingMatrixOverrideName, GetCullingMatrixOverride(container.Version).ExportYAML(container));
				node.Add(UseCullingMatrixOverrideName, UseCullingMatrixOverride);
			}
			return node;
		}

		private Matrix4x4f GetCullingMatrixOverride(Version version)
		{
			return IsReadCullingMatrixOverride(version) ? CullingMatrixOverride : Matrix4x4f.Identity;
		}

		public LightShadows Type { get; private set; }
		public int Resolution { get; private set; }
		public int CustomResolution { get; private set; }
		public float Strength { get; private set; }
		public int Projection { get; private set; }
		public float ConstantBias { get; private set; }
		public float ObjectSizeBias { get; private set; }
		public float Bias { get; private set; }
		public float Softness  { get; private set; }
		public float SoftnessFade  { get; private set; }
		public float NormalBias { get; private set; }
		public float NearPlane { get; private set; }
		public bool UseCullingMatrixOverride { get; private set; }
		
		public const string TypeName = "m_Type";
		public const string ResolutionName = "m_Resolution";
		public const string CustomResolutionName = "m_CustomResolution";
		public const string StrengthName = "m_Strength";
		public const string BiasName = "m_Bias";
		public const string NormalBiasName = "m_NormalBias";
		public const string NearPlaneName = "m_NearPlane";
		public const string CullingMatrixOverrideName = "m_CullingMatrixOverride";
		public const string UseCullingMatrixOverrideName = "m_UseCullingMatrixOverride";

		public Matrix4x4f CullingMatrixOverride { get; private set; }
	}
}
