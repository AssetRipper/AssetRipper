using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.Converters.Misc;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct KeyframeTpl<T> : IAsset
		where T : struct, IAsset
	{
		public KeyframeTpl(float time, T value, T weight):
			this(time, value, default, default, weight)
		{
			// this enum member is version agnostic
			TangentMode = AnimationClips.TangentMode.FreeSmooth.ToTangent(Version.MinVersion);
		}

		public KeyframeTpl(float time, T value, T inSlope, T outSlope, T weight)
		{
			Time = time;
			Value = value;
			InSlope = inSlope;
			OutSlope = outSlope;
			// this enum member is version agnostic
			TangentMode = AnimationClips.TangentMode.FreeFree.ToTangent(Version.MinVersion);
			WeightedMode = WeightedMode.None;
			InWeight = weight;
			OutWeight = weight;
		}

		public static int ToSerializedVersion(Version version)
		{
			// unknown conversion
			if (version.IsGreaterEqual(2018))
			{
				return 3;
			}
			// TangentMode enum has been changed
			if (TangentModeExtensions.TangentMode5Relevant(version))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 2.1.0 and greater and Not Release
		/// </summary>
		public static bool HasTangentMode(Version version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(2, 1);
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool HasWeight(Version version) => version.IsGreaterEqual(2018);

		public KeyframeTpl<T> Convert(IExportContainer container)
		{
			return KeyframeTplConverter.Convert(container, ref this);
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
			if (HasWeight(reader.Version))
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
			if (HasWeight(writer.Version))
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
			if (HasWeight(container.ExportVersion))
			{
				node.Add(WeightedModeName, (int)WeightedMode);
				node.Add(InWeightName, InWeight.ExportYAML(container));
				node.Add(OutWeightName, OutWeight.ExportYAML(container));
			}
			return node;
		}

		public TangentMode GetTangentMode(Version version)
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

		public float Time { get; set; }
		public int TangentMode { get; set; }
		public WeightedMode WeightedMode { get; set; }

		public const string TimeName = "time";
		public const string ValueName = "value";
		public const string InSlopeName = "inSlope";
		public const string OutSlopeName = "outSlope";
		public const string TangentModeName = "tangentMode";
		public const string WeightedModeName = "weightedMode";
		public const string InWeightName = "inWeight";
		public const string OutWeightName = "outWeight";

		public static Float DefaultFloatWeight => 1.0f / 3.0f;
		public static Vector3f DefaultVector3Weight => new Vector3f(1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 3.0f);
		public static Quaternionf DefaultQuaternionWeight => new Quaternionf(1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 3.0f);

		public T Value;
		public T InSlope;
		public T OutSlope;
		public T InWeight;
		public T OutWeight;
	}
}
