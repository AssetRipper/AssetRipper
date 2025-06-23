using AssetRipper.IO.Endian;
using System.Diagnostics;

namespace AssetRipper.Processing.AnimationClips.Editor;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed class StreamedFrame
{
	public void Read(ref EndianSpanReader reader, UnityVersion version)
	{
		Time = reader.ReadSingle();
		Curves = ReadAssetArray(ref reader, version);
	}

	public float Time { get; set; }
	public StreamedCurveKey[] Curves { get; set; } = Array.Empty<StreamedCurveKey>();

	private static StreamedCurveKey[] ReadAssetArray(ref EndianSpanReader reader, UnityVersion version)
	{
		int curveCount = reader.ReadInt32();
		ThrowIfNegative(curveCount);

		StreamedCurveKey[] curvesArray = curveCount == 0 ? Array.Empty<StreamedCurveKey>() : new StreamedCurveKey[curveCount];
		if (curveCount > 0)
		{
			StreamedCurveKey instance = ReadNextCurve(ref reader);
			curvesArray[0] = instance;

			int saveIdx = 1;
			for (int readIdx = 1; readIdx < curveCount; readIdx++, saveIdx++)
			{
				instance = ReadNextCurve(ref reader);
				if (curvesArray[saveIdx - 1].Index == instance.Index)
				{
					saveIdx--; // keep only last curve from sequence of duplicated binding/Index
				}
				curvesArray[saveIdx] = instance;
			}
			if (saveIdx != curveCount)
			{
				curvesArray = curvesArray.AsSpan(0, saveIdx).ToArray();
			}
		}

		if (version.GreaterThanOrEquals(2017, 1))
		{
			reader.Align();
		}
		return curvesArray;

		static StreamedCurveKey ReadNextCurve(ref EndianSpanReader reader)
		{
			StreamedCurveKey instance = new();
			instance.Read(ref reader);
			return instance;
		}

		static void ThrowIfNegative(int count)
		{
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), $"Cannot be negative: {count}");
			}
		}
	}

	private string GetDebuggerDisplay()
	{
		return $$"""{ {{nameof(Time)}} : {{Time}}, {{nameof(Curves)}}.Length : {{Curves?.Length}} }""";
	}
}
