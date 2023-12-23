using AssetRipper.IO.Endian;

namespace AssetRipper.Processing.AnimationClips.Editor
{
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
			int count = reader.ReadInt32();
			ThrowIfNegative(count);

			StreamedCurveKey[] array = count == 0 ? Array.Empty<StreamedCurveKey>() : new StreamedCurveKey[count];
			for (int i = 0; i < count; i++)
			{
				StreamedCurveKey instance = new();
				instance.Read(ref reader);
				array[i] = instance;
			}
			if (version.GreaterThanOrEquals(2017, 1))
			{
				reader.Align();
			}
			return array;

			static void ThrowIfNegative(int count)
			{
				if (count < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(count), $"Cannot be negative: {count}");
				}
			}
		}
	}
}
