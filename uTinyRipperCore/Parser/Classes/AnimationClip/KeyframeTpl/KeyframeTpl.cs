using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct KeyframeTpl<T> : IAssetReadable, IYAMLExportable
		where T : struct, IAssetReadable, IYAMLExportable
	{
		public KeyframeTpl(float time, T value, T weight):
			this(time, value, default, default, weight)
		{
		}

		public KeyframeTpl(float time, T value, T inSlope, T outSlope, T weight)
		{
			Time = time;
			Value = value;
			InSlope = inSlope;
			OutSlope = outSlope;
#if UNIVERSAL
			TangentMode = TangentMode.FreeSmooth;
#endif
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
		public static bool IsReadTangentMode(Version version, TransferInstructionFlags flags)
		{
			return GetSerializedVersion(version) >= 2 && !flags.IsRelease();
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
#warning TODO: 2018
				//return 3;
				return 2;
			}

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
#if UNIVERSAL
			if(IsReadTangentMode(reader.Version, reader.Flags))
			{
				TangentMode = (TangentMode)reader.ReadInt32();
			}
#endif
			if(IsReadWeight(reader.Version))
			{
				WeightedMode = (WeightedMode)reader.ReadInt32();
				InWeight.Read(reader);
				OutWeight.Read(reader);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("time", Time);
			node.Add("value", Value.ExportYAML(container));
			node.Add("inSlope", InSlope.ExportYAML(container));
			node.Add("outSlope", OutSlope.ExportYAML(container));
			if (GetSerializedVersion(container.Version) >= 2)
			{
				node.Add("tangentMode", (int)GetTangentMode(container.Version, container.Flags));
			}
			if (GetSerializedVersion(container.Version) >= 3)
			{
				node.Add("weightedMode", (int)WeightedMode);
				node.Add("inWeight", InWeight.ExportYAML(container));
				node.Add("outWeight", OutWeight.ExportYAML(container));
			}
			return node;
		}

		private TangentMode GetTangentMode(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if(IsReadTangentMode(version, flags))
			{
				return TangentMode;
			}
#endif
			return TangentMode.FreeSmooth;
		}

		public float Time { get; private set; }
#if UNIVERSAL
		public TangentMode TangentMode { get; private set; }
#endif
		public WeightedMode WeightedMode { get; private set; }

		public T Value;
		public T InSlope;
		public T OutSlope;
		public T InWeight;
		public T OutWeight;
	}
}
