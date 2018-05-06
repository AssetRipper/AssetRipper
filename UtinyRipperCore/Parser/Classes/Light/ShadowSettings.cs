using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.Lights
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
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadNearPlane(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}

		public void Read(AssetStream stream)
		{
			Type = (LightShadows)stream.ReadInt32();
			Resolution = stream.ReadInt32();
			if (IsReadCustomResolution(stream.Version))
			{
				CustomResolution = stream.ReadInt32();
			}
			Strength = stream.ReadSingle();
			if (IsReadProjection(stream.Version))
			{
				Projection = stream.ReadInt32();
			}
			if (IsReadConstantBias(stream.Version))
			{
				ConstantBias = stream.ReadSingle();
				ObjectSizeBias = stream.ReadSingle();
			}
			if (IsReadBias(stream.Version))
			{
				Bias = stream.ReadSingle();
			}
			if (IsReadSoftness(stream.Version))
			{
				Softness = stream.ReadSingle();
				SoftnessFade = stream.ReadSingle();
			}
			if (IsReadNormalBias(stream.Version))
			{
				NormalBias = stream.ReadSingle();
			}
			if (IsReadNearPlane(stream.Version))
			{
				NearPlane = stream.ReadSingle();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Type", (int)Type);
			node.Add("m_Resolution", Resolution);
			node.Add("m_CustomResolution", CustomResolution);
			node.Add("m_Strength", Strength);
			node.Add("m_Bias", Bias);
			node.Add("m_NormalBias", NormalBias);
			node.Add("m_NearPlane", NearPlane);
			return node;
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
	}
}
