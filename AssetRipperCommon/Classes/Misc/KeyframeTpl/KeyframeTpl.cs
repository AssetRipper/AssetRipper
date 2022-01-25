﻿using AssetRipper.Core.Classes.Misc.KeyframeTpl.TangentMode;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Misc.KeyframeTpl
{
	public struct KeyframeTpl<T> : IAsset
		where T : struct, IAsset
	{
		public KeyframeTpl(float time, T value, T weight) : this(time, value, default, default, weight)
		{
			// this enum member is version agnostic
			TangentMode = KeyframeTpl.TangentMode.TangentMode.FreeSmooth.ToTangent(UnityVersion.MinVersion);
		}

		public KeyframeTpl(float time, T value, T inSlope, T outSlope, T weight)
		{
			Time = time;
			Value = value;
			InSlope = inSlope;
			OutSlope = outSlope;
			// this enum member is version agnostic
			TangentMode = KeyframeTpl.TangentMode.TangentMode.FreeFree.ToTangent(UnityVersion.MinVersion);
			WeightedMode = WeightedMode.None;
			InWeight = weight;
			OutWeight = weight;
		}

		public void Read(AssetReader reader)
		{
			Time = reader.ReadSingle();
			Value.Read(reader);
			InSlope.Read(reader);
			OutSlope.Read(reader);
			if (HasTangentMode(reader.Version, reader.Flags))
			{
				TangentMode = reader.ReadInt32();
			}
			if (HasWeightedMode(reader.Version))
			{
				WeightedMode = (WeightedMode)reader.ReadInt32();
				InWeight.Read(reader);
				OutWeight.Read(reader);
			}
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(Time);
			Value.Write(writer);
			InSlope.Write(writer);
			OutSlope.Write(writer);
			if (HasTangentMode(writer.Version, writer.Flags))
			{
				writer.Write(TangentMode);
			}
			if (HasWeightedMode(writer.Version))
			{
				writer.Write((int)WeightedMode);
				InWeight.Write(writer);
				OutWeight.Write(writer);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(TimeName, Time);
			node.Add(ValueName, Value.ExportYAML(container));
			node.Add(InSlopeName, InSlope.ExportYAML(container));
			node.Add(OutSlopeName, OutSlope.ExportYAML(container));
			if (HasTangentMode(container.ExportVersion, container.ExportFlags))
			{
				node.Add(TangentModeName, TangentMode);
			}
			if (HasWeightedMode(container.ExportVersion))
			{
				node.Add(WeightedModeName, (int)WeightedMode);
				node.Add(InWeightName, InWeight.ExportYAML(container));
				node.Add(OutWeightName, OutWeight.ExportYAML(container));
			}
			return node;
		}

		public TangentMode.TangentMode GetTangentMode(UnityVersion version)
		{
			if (TangentModeExtensions.TangentMode5Relevant(version))
			{
				return ((TangentMode5)TangentMode).ToTangentMode();
			}
			else
			{
				return ((TangentMode2)TangentMode).ToTangentMode();
			}
		}

		public static int ToSerializedVersion(UnityVersion version)
		{
			if (version.IsGreaterEqual(2018))
			{
				// unknown conversion
				return 3;
			}
			else if (TangentModeExtensions.TangentMode5Relevant(version))
			{
				// TangentMode enum has been changed
				return 2;
			}
			else
			{
				return 1;
			}
		}

		/// <summary>
		/// 2.1.0 and greater and Not Release
		/// </summary>
		public static bool HasTangentMode(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(2, 1) && !flags.IsRelease();
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool HasWeightedMode(UnityVersion version) => version.IsGreaterEqual(2018);
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool HasInWeight(UnityVersion version) => version.IsGreaterEqual(2018);
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool HasOutWeight(UnityVersion version) => version.IsGreaterEqual(2018);

		public float Time { get; set; }
		public int TangentMode { get; set; }
		public WeightedMode WeightedMode { get; set; }

		public static Float DefaultFloatWeight => 1.0f / 3.0f;
		public static Vector3f DefaultVector3Weight => new Vector3f(1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 3.0f);
		public static Quaternionf DefaultQuaternionWeight => new Quaternionf(1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 3.0f);

		public T Value;
		public T InSlope;
		public T OutSlope;
		public T InWeight;
		public T OutWeight;

		public const string TimeName = "time";
		public const string ValueName = "value";
		public const string InSlopeName = "inSlope";
		public const string OutSlopeName = "outSlope";
		public const string TangentModeName = "tangentMode";
		public const string WeightedModeName = "weightedMode";
		public const string InWeightName = "inWeight";
		public const string OutWeightName = "outWeight";
	}
}
