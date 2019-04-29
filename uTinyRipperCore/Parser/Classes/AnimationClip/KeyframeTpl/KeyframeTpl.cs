using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct KeyframeTpl<T> : IAssetReadable, IYAMLExportable
		where T : struct, IAssetReadable, IYAMLExportable
	{
		public KeyframeTpl(float time, T value, T weight):
			this(time, value, default, default, weight)
		{
			TangentMode = TangentMode.FreeSmooth;
		}

		public KeyframeTpl(float time, T value, T inSlope, T outSlope, T weight)
		{
			Time = time;
			Value = value;
			InSlope = inSlope;
			OutSlope = outSlope;
			TangentMode = TangentMode.FreeFree;
			WeightedMode = WeightedMode.None;
			InWeight = weight;
			OutWeight = weight;
		}

		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool IsReadWeight(Version version)
		{
			return version.IsGreaterEqual(2018);
		}
		/// <summary>
		/// 5.5.0 and greater and Not Release
		/// </summary>
		public static bool IsReadTangentMode(Version version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(5, 5) && !flags.IsRelease();
		}

		private static int GetSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(2018))
			{
				return 3;
			}
			if (version.IsGreaterEqual(5, 5))
			{
				return 2;
			}
			return 1;
		}

		public void Read(AssetReader reader)
		{
			Time = reader.ReadSingle();
			Value.Read(reader);
			InSlope.Read(reader);
			OutSlope.Read(reader);
			if (IsReadTangentMode(reader.Version, reader.Flags))
			{
				TangentMode = (TangentMode)reader.ReadInt32();
			}
			if (IsReadWeight(reader.Version))
			{
				WeightedMode = (WeightedMode)reader.ReadInt32();
				InWeight.Read(reader);
				OutWeight.Read(reader);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(TimeName, Time);
			node.Add(ValueName, Value.ExportYAML(container));
			node.Add(InSlopeName, InSlope.ExportYAML(container));
			node.Add(OutSlopeName, OutSlope.ExportYAML(container));
			if (IsReadTangentMode(container.ExportVersion, container.ExportFlags))
			{
				node.Add(TangentModeName, (int)TangentMode);
			}
			if (IsReadWeight(container.ExportVersion))
			{
				node.Add(WeightedModeName, (int)WeightedMode);
				node.Add(InWeightName, InWeight.ExportYAML(container));
				node.Add(OutWeightName, OutWeight.ExportYAML(container));
			}
			return node;
		}

		public float Time { get; private set; }
		public TangentMode TangentMode { get; private set; }
		public WeightedMode WeightedMode { get; private set; }

		public const string TimeName = "time";
		public const string ValueName = "value";
		public const string InSlopeName = "inSlope";
		public const string OutSlopeName = "outSlope";
		public const string TangentModeName = "tangentMode";
		public const string WeightedModeName = "weightedMode";
		public const string InWeightName = "inWeight";
		public const string OutWeightName = "outWeight";

		public T Value;
		public T InSlope;
		public T OutSlope;
		public T InWeight;
		public T OutWeight;
	}
}
