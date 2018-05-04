using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.AnimationClips;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct MinMaxCurve : IAssetReadable, IYAMLExportable
	{
		public MinMaxCurve(float value)
		{
			MinMaxState = ParticleSystemCurveMode.Constant;
			Scalar = value;
			MinScalar = value;

			MinCurve = new AnimationCurveTpl<Float>(default(Float));
			MaxCurve = new AnimationCurveTpl<Float>(default(Float));
		}

		public MinMaxCurve(float minValue, float maxValue):
			this(maxValue)
		{
			MinMaxState = ParticleSystemCurveMode.TwoConstants;
			Scalar = maxValue;
			MinScalar = minValue;

			MinCurve = new AnimationCurveTpl<Float>(default(Float));
			MaxCurve = new AnimationCurveTpl<Float>(default(Float));
		}

		/// <summary>
		/// 5.6.1 and greater
		/// </summary>
		public static bool IsReadMinScalar(Version version)
		{
			return version.IsGreaterEqual(5, 6, 1);
		}

		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		private static bool IsMinMaxStateFirst(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

			if (version.IsGreaterEqual(5, 6, 1))
			{
				return 2;
			}
			return 1;
		}

		private float GetExportMinScalar(Version version)
		{
			return IsReadMinScalar(version) ? MinScalar : Scalar;
		}

		public void Read(AssetStream stream)
		{
			if (IsMinMaxStateFirst(stream.Version))
			{
				MinMaxState = (ParticleSystemCurveMode)stream.ReadUInt16();
				stream.AlignStream(AlignType.Align4);
			}
			
			Scalar = stream.ReadSingle();
			if (IsReadMinScalar(stream.Version))
			{
				MinScalar = stream.ReadSingle();
			}
			MaxCurve.Read(stream);
			MinCurve.Read(stream);
			
			if (!IsMinMaxStateFirst(stream.Version))
			{
				MinMaxState = (ParticleSystemCurveMode)stream.ReadUInt16();
				stream.AlignStream(AlignType.Align4);
			}
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("minMaxState", (ushort)MinMaxState);
			node.Add("scalar", Scalar);
			node.Add("minScalar", GetExportMinScalar(exporter.Version));
			node.Add("maxCurve", MaxCurve.ExportYAML(exporter));
			node.Add("minCurve", MinCurve.ExportYAML(exporter));
			return node;
		}

		public ParticleSystemCurveMode MinMaxState { get; private set; }
		public float Scalar { get; private set; }
		public float MinScalar { get; private set; }

		public AnimationCurveTpl<Float> MaxCurve;
		public AnimationCurveTpl<Float> MinCurve;
	}
}
