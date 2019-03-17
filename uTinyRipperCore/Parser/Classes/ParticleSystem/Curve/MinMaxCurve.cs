using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.AnimationClips;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ParticleSystems
{
	public struct MinMaxCurve : IAssetReadable, IYAMLExportable
	{
		public MinMaxCurve(float value) :
			this(ParticleSystemCurveMode.Constant, value, value, 1.0f, 1.0f)
		{
		}

		public MinMaxCurve(float minValue, float maxValue) :
			this(ParticleSystemCurveMode.Constant, minValue, maxValue, 1.0f, 1.0f)
		{
		}

		public MinMaxCurve(float minValue, float maxValue, float minCurve, float maxCurve):
			this(ParticleSystemCurveMode.Constant, minValue, maxValue, minCurve, maxCurve)
		{
		}

		public MinMaxCurve(float minValue, float maxValue, float minCurve, float maxCurve1, float maxCurve2)
		{
			MinMaxState = ParticleSystemCurveMode.Curve;
			Scalar = maxValue;
			MinScalar = minValue;

			MinCurve = new AnimationCurveTpl<Float>(minCurve, Float.DefaultWeight);
			MaxCurve = new AnimationCurveTpl<Float>(maxCurve1, 0.0f, 1.0f, maxCurve2, 1.0f, 0.0f, Float.DefaultWeight);
		}

		public MinMaxCurve(ParticleSystemCurveMode mode, float minValue, float maxValue, float minCurve, float maxCurve)
		{
			MinMaxState = mode;
			MinScalar = minValue;
			Scalar = maxValue;

			MinCurve = new AnimationCurveTpl<Float>(minCurve, Float.DefaultWeight);
			MaxCurve = new AnimationCurveTpl<Float>(maxCurve, Float.DefaultWeight);
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

		public void Read(AssetReader reader)
		{
			if (IsMinMaxStateFirst(reader.Version))
			{
				MinMaxState = (ParticleSystemCurveMode)reader.ReadUInt16();
				reader.AlignStream(AlignType.Align4);
			}
			
			Scalar = reader.ReadSingle();
			MinScalar = IsReadMinScalar(reader.Version) ? reader.ReadSingle() : Scalar;
			MaxCurve.Read(reader);
			MinCurve.Read(reader);
			
			if (!IsMinMaxStateFirst(reader.Version))
			{
				MinMaxState = (ParticleSystemCurveMode)reader.ReadUInt16();
				reader.AlignStream(AlignType.Align4);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("minMaxState", (ushort)MinMaxState);
			node.Add("scalar", GetExportScalar(container.Version));
			node.Add("minScalar", GetExportMinScalar(container.Version));
			node.Add("maxCurve", MaxCurve.ExportYAML(container));
			node.Add("minCurve", MinCurve.ExportYAML(container));
			return node;
		}

		private float GetExportScalar(Version version)
		{
			if(IsReadMinScalar(version))
			{
				return Scalar;
			}
			else
			{
				if (MinMaxState == ParticleSystemCurveMode.TwoConstants)
				{
					return Scalar * MaxCurve.Curve[0].Value.Value;
				}
				else
				{
					return Scalar;
				}
			}
		}
		private float GetExportMinScalar(Version version)
		{
			if(IsReadMinScalar(version))
			{
				return MinScalar;
			}
			else
			{
				if (MinMaxState == ParticleSystemCurveMode.TwoConstants)
				{
					return Scalar * MinCurve.Curve[0].Value.Value;
				}
				else
				{
					return MinScalar;
				}
			}
		}

		public ParticleSystemCurveMode MinMaxState { get; private set; }
		public float Scalar { get; private set; }
		public float MinScalar { get; private set; }

		public AnimationCurveTpl<Float> MaxCurve;
		public AnimationCurveTpl<Float> MinCurve;
	}
}
