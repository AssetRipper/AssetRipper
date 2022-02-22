using AssetRipper.Core.Classes.AnimationClip.Curves;
using AssetRipper.Core.Classes.Misc.KeyframeTpl;
using AssetRipper.Core.Classes.Misc.Serializable.AnimationCurveTpl;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.PackedBitVectors;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.AnimationClip
{
	public sealed class CompressedAnimationCurve : IAssetReadable, IYAMLExportable
	{
		public QuaternionCurve Unpack()
		{
			int[] timesValues = Times.UnpackInts();
			float[] times = new float[timesValues.Length];
			for (int i = 0; i < times.Length; i++)
			{
				times[i] = timesValues[i] * 0.01f;
			}
			Quaternionf[] rotations = Values.Unpack();
			float[] slopes = Slopes.Unpack();

			KeyframeTpl<Quaternionf>[] keyframes = new KeyframeTpl<Quaternionf>[rotations.Length];
			for (int i = 0, j = 4; i < rotations.Length; i++, j += 4)
			{
				float time = times[i];
				Quaternionf rotation = rotations[i];
				Quaternionf inSlope = new Quaternionf(slopes[j - 4], slopes[j - 3], slopes[j - 2], slopes[j - 1]);
				Quaternionf outSlope = new Quaternionf(slopes[j + 0], slopes[j + 1], slopes[j + 2], slopes[j + 3]);
				keyframes[i] = new KeyframeTpl<Quaternionf>(time, rotation, inSlope, outSlope, KeyframeTpl<Quaternionf>.DefaultQuaternionWeight);
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
			node.Add(PathName, Path);
			node.Add(TimesName, Times.ExportYAML(container));
			node.Add(ValuesName, Values.ExportYAML(container));
			node.Add(SlopesName, Slopes.ExportYAML(container));
			node.Add(PreInfinityName, (int)PreInfinity);
			node.Add(PostInfinityName, (int)PostInfinity);
			return node;
		}

		public string Path { get; set; }
		public CurveLoopTypes PreInfinity { get; set; }
		public CurveLoopTypes PostInfinity { get; set; }

		public const string PathName = "m_Path";
		public const string TimesName = "m_Times";
		public const string ValuesName = "m_Values";
		public const string SlopesName = "m_Slopes";
		public const string PreInfinityName = "m_PreInfinity";
		public const string PostInfinityName = "m_PostInfinity";

		public PackedIntVector Times = new();
		public PackedQuatVector Values = new();
		public PackedFloatVector Slopes = new();
	}
}
