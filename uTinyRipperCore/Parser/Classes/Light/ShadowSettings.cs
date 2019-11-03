using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Lights
{
	public struct ShadowSettings : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasCustomResolution(Version version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// Less than 3.4.0
		/// </summary>
		public static bool HasProjection(Version version) => version.IsLess(3, 4);
		/// <summary>
		/// Less than 3
		/// </summary>
		public static bool HasConstantBias(Version version) => version.IsLess(3);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasBias(Version version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 3.2.0 to 5.0.0beta
		/// </summary>
		public static bool HasSoftness(Version version) => version.IsGreaterEqual(3, 2) && version.IsLess(5, 0, 0, VersionType.Beta);
		/// <summary>
		/// 5.0.0f and greater
		/// </summary>
		public static bool HasNormalBias(Version version) => version.IsGreater(5, 0, 0, VersionType.Beta);
		/// <summary>
		/// 5.3.0b6 and greater
		/// </summary>
		public static bool HasNearPlane(Version version) => version.IsGreaterEqual(5, 3, 0, VersionType.Beta, 6);
		/// <summary>
		/// 2019.1.0b4 and greater
		/// </summary>
		public static bool HasCullingMatrixOverride(Version version) => version.IsGreaterEqual(2019, 1, 0, VersionType.Beta, 4);

		public void Read(AssetReader reader)
		{
			Type = (LightShadows)reader.ReadInt32();
			Resolution = reader.ReadInt32();
			if (HasCustomResolution(reader.Version))
			{
				CustomResolution = reader.ReadInt32();
			}
			Strength = reader.ReadSingle();
			if (HasProjection(reader.Version))
			{
				Projection = reader.ReadInt32();
			}
			if (HasConstantBias(reader.Version))
			{
				ConstantBias = reader.ReadSingle();
				ObjectSizeBias = reader.ReadSingle();
			}
			if (HasBias(reader.Version))
			{
				Bias = reader.ReadSingle();
			}
			if (HasSoftness(reader.Version))
			{
				Softness = reader.ReadSingle();
				SoftnessFade = reader.ReadSingle();
			}
			if (HasNormalBias(reader.Version))
			{
				NormalBias = reader.ReadSingle();
			}
			if (HasNearPlane(reader.Version))
			{
				NearPlane = reader.ReadSingle();
			}
			if (HasCullingMatrixOverride(reader.Version))
			{
				CullingMatrixOverride.Read(reader);
				UseCullingMatrixOverride = reader.ReadBoolean();
				reader.AlignStream();
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
			if (HasCullingMatrixOverride(container.ExportVersion))
			{
				node.Add(CullingMatrixOverrideName, GetCullingMatrixOverride(container.Version).ExportYAML(container));
				node.Add(UseCullingMatrixOverrideName, UseCullingMatrixOverride);
			}
			return node;
		}

		private Matrix4x4f GetCullingMatrixOverride(Version version)
		{
			return HasCullingMatrixOverride(version) ? CullingMatrixOverride : Matrix4x4f.Identity;
		}

		public LightShadows Type { get; set; }
		public int Resolution { get; set; }
		public int CustomResolution { get; set; }
		public float Strength { get; set; }
		public int Projection { get; set; }
		public float ConstantBias { get; set; }
		public float ObjectSizeBias { get; set; }
		public float Bias { get; set; }
		public float Softness  { get; set; }
		public float SoftnessFade  { get; set; }
		public float NormalBias { get; set; }
		public float NearPlane { get; set; }
		public bool UseCullingMatrixOverride { get; set; }
		
		public const string TypeName = "m_Type";
		public const string ResolutionName = "m_Resolution";
		public const string CustomResolutionName = "m_CustomResolution";
		public const string StrengthName = "m_Strength";
		public const string BiasName = "m_Bias";
		public const string NormalBiasName = "m_NormalBias";
		public const string NearPlaneName = "m_NearPlane";
		public const string CullingMatrixOverrideName = "m_CullingMatrixOverride";
		public const string UseCullingMatrixOverrideName = "m_UseCullingMatrixOverride";

		public Matrix4x4f CullingMatrixOverride { get; set; }
	}
}
