using uTinyRipper.YAML;
using uTinyRipper.Converters.Misc;
using uTinyRipper.Converters;
using uTinyRipper.Layout.Misc;

namespace uTinyRipper.Classes.Misc
{
	public struct KeyframeTpl<T> : IAsset
		where T : struct, IAsset
	{
		public KeyframeTpl(float time, T value, T weight) :
			this(time, value, default, default, weight)
		{
			// this enum member is version agnostic
			TangentMode = Misc.TangentMode.FreeSmooth.ToTangent(Version.MinVersion);
		}

		public KeyframeTpl(float time, T value, T inSlope, T outSlope, T weight)
		{
			Time = time;
			Value = value;
			InSlope = inSlope;
			OutSlope = outSlope;
			// this enum member is version agnostic
			TangentMode = Misc.TangentMode.FreeFree.ToTangent(Version.MinVersion);
			WeightedMode = WeightedMode.None;
			InWeight = weight;
			OutWeight = weight;
		}

		public KeyframeTpl<T> Convert(IExportContainer container)
		{
			return KeyframeTplConverter.Convert(container, ref this);
		}

		public void Read(AssetReader reader)
		{
			KeyframeTplLayout layout = reader.Layout.Misc.KeyframeTpl;
			Time = reader.ReadSingle();
			Value.Read(reader);
			InSlope.Read(reader);
			OutSlope.Read(reader);
			if (layout.HasTangentMode)
			{
				TangentMode = reader.ReadInt32();
			}
			if (layout.HasWeightedMode)
			{
				WeightedMode = (WeightedMode)reader.ReadInt32();
				InWeight.Read(reader);
				OutWeight.Read(reader);
			}
		}

		public void Write(AssetWriter writer)
		{
			KeyframeTplLayout layout = writer.Layout.Misc.KeyframeTpl;
			writer.Write(Time);
			Value.Write(writer);
			InSlope.Write(writer);
			OutSlope.Write(writer);
			if (layout.HasTangentMode)
			{
				writer.Write(TangentMode);
			}
			if (layout.HasWeightedMode)
			{
				writer.Write((int)WeightedMode);
				InWeight.Write(writer);
				OutWeight.Write(writer);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			KeyframeTplLayout layout = container.ExportLayout.Misc.KeyframeTpl;
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(layout.Version);
			node.Add(layout.TimeName, Time);
			node.Add(layout.ValueName, Value.ExportYAML(container));
			node.Add(layout.InSlopeName, InSlope.ExportYAML(container));
			node.Add(layout.OutSlopeName, OutSlope.ExportYAML(container));
			if (layout.HasTangentMode)
			{
				node.Add(layout.TangentModeName, TangentMode);
			}
			if (layout.HasWeightedMode)
			{
				node.Add(layout.WeightedModeName, (int)WeightedMode);
				node.Add(layout.InWeightName, InWeight.ExportYAML(container));
				node.Add(layout.OutWeightName, OutWeight.ExportYAML(container));
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
