﻿using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Misc.KeyframeTpl;
using AssetRipper.Core.Classes.Misc.Serializable.AnimationCurveTpl;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.ParticleSystem.Curve
{
	public sealed class MinMaxCurve : IAssetReadable, IYamlExportable
	{
		public MinMaxCurve() { }

		public MinMaxCurve(float value) : this(ParticleSystemCurveMode.Constant, value, value, 1.0f, 1.0f) { }

		public MinMaxCurve(float minValue, float maxValue) : this(ParticleSystemCurveMode.Constant, minValue, maxValue, 1.0f, 1.0f) { }

		public MinMaxCurve(float minValue, float maxValue, float minCurve, float maxCurve) : this(ParticleSystemCurveMode.Constant, minValue, maxValue, minCurve, maxCurve) { }

		public MinMaxCurve(float minValue, float maxValue, float minCurve, float maxCurve1, float maxCurve2)
		{
			MinMaxState = ParticleSystemCurveMode.Curve;
			Scalar = maxValue;
			MinScalar = minValue;

			MinCurve = new AnimationCurveTpl<Float>(minCurve, KeyframeTpl<Float>.DefaultFloatWeight);
			MaxCurve = new AnimationCurveTpl<Float>(maxCurve1, 0.0f, 1.0f, maxCurve2, 1.0f, 0.0f, KeyframeTpl<Float>.DefaultFloatWeight);
		}

		public MinMaxCurve(ParticleSystemCurveMode mode, float minValue, float maxValue, float minCurve, float maxCurve)
		{
			MinMaxState = mode;
			MinScalar = minValue;
			Scalar = maxValue;

			MinCurve = new AnimationCurveTpl<Float>(minCurve, KeyframeTpl<Float>.DefaultFloatWeight);
			MaxCurve = new AnimationCurveTpl<Float>(maxCurve, KeyframeTpl<Float>.DefaultFloatWeight);
		}

		public static int ToSerializedVersion(UnityVersion version)
		{
			if (version.IsGreaterEqual(5, 6, 0, UnityVersionType.Patch, 4))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 5.6.0p4 and greater
		/// </summary>
		public static bool HasMinScalar(UnityVersion version) => version.IsGreaterEqual(5, 6, 0, UnityVersionType.Patch, 4);

		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		private static bool IsMinMaxStateFirst(UnityVersion version) => version.IsGreaterEqual(5, 6);

		public void Read(AssetReader reader)
		{
			if (IsMinMaxStateFirst(reader.Version))
			{
				MinMaxState = (ParticleSystemCurveMode)reader.ReadUInt16();
				reader.AlignStream();
			}

			Scalar = reader.ReadSingle();
			MinScalar = HasMinScalar(reader.Version) ? reader.ReadSingle() : Scalar;
			MaxCurve.Read(reader);
			MinCurve.Read(reader);

			if (!IsMinMaxStateFirst(reader.Version))
			{
				MinMaxState = (ParticleSystemCurveMode)reader.ReadUInt16();
				reader.AlignStream();
			}
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(MinMaxStateName, (ushort)MinMaxState);
			node.Add(ScalarName, GetExportScalar(container.Version));
			node.Add(MinScalarName, GetExportMinScalar(container.Version));
			node.Add(MaxCurveName, MaxCurve.ExportYaml(container));
			node.Add(MinCurveName, MinCurve.ExportYaml(container));
			return node;
		}

		private float GetExportScalar(UnityVersion version)
		{
			if (HasMinScalar(version))
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
		private float GetExportMinScalar(UnityVersion version)
		{
			if (HasMinScalar(version))
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

		public ParticleSystemCurveMode MinMaxState { get; set; }
		public float Scalar { get; set; }
		public float MinScalar { get; set; }

		public const string MinMaxStateName = "minMaxState";
		public const string ScalarName = "scalar";
		public const string MinScalarName = "minScalar";
		public const string MaxCurveName = "maxCurve";
		public const string MinCurveName = "minCurve";

		public AnimationCurveTpl<Float> MaxCurve = new();
		public AnimationCurveTpl<Float> MinCurve = new();
	}
}
