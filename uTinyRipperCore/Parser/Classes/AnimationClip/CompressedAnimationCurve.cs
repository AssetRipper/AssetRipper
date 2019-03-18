using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct CompressedAnimationCurve : IAssetReadable, IYAMLExportable
	{
		public QuaternionCurve Unpack()
		{
			int[] timesValues = Times.Unpack();
			float[] times = new float[timesValues.Length];
			for(int i = 0; i < times.Length; i++)
			{
				times[i] = timesValues[i] * 0.01f;
			}
			Quaternionf[] rotations = Values.Unpack();
			float[] slopes = Slopes.Unpack();

			KeyframeTpl<Quaternionf>[] keyframes = new KeyframeTpl<Quaternionf>[rotations.Length];
			for(int i = 0, j = 4; i < rotations.Length; i++, j += 4)
			{
				float time = times[i];
				Quaternionf rotation = rotations[i];
				Quaternionf inSlope = new Quaternionf(slopes[j - 4], slopes[j - 3], slopes[j - 2], slopes[j - 1]);
				Quaternionf outSlope = new Quaternionf(slopes[j + 0], slopes[j + 1], slopes[j + 2], slopes[j + 3]);
				keyframes[i] = new KeyframeTpl<Quaternionf>(time, rotation, inSlope, outSlope, Quaternionf.DefaultWeight);
			}
			AnimationCurveTpl<Quaternionf> curve = new AnimationCurveTpl<Quaternionf>(keyframes, PreInfinity, PostInfinity);
			return new QuaternionCurve(Path, curve);
		}

		public void Read(AssetReader reader)
		{
			Path = reader.ReadString();
			Times.Read(reader);
			Values.Read(reader);
			Slopes.Read(reader);
			PreInfinity = (CurveLoopTypes)reader.ReadInt32();
			PostInfinity = (CurveLoopTypes)reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Path", Path);
			node.Add("m_Times", Times.ExportYAML(container));
			node.Add("m_Values", Values.ExportYAML(container));
			node.Add("m_Slopes", Slopes.ExportYAML(container));
			node.Add("m_PreInfinity", (int)PreInfinity);
			node.Add("m_PostInfinity", (int)PostInfinity);
			return node;
		}

		public string Path { get; private set; }
		public CurveLoopTypes PreInfinity { get; private set; }
		public CurveLoopTypes PostInfinity { get; private set; }

		public PackedIntVector Times;
		public PackedQuatVector Values;
		public PackedFloatVector Slopes;
	}
}
