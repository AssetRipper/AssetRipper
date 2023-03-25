using AssetRipper.Assets.IO.Reading;

namespace AssetRipper.Processing.AnimationClips.Editor
{
	public sealed class StreamedFrame
	{
		public void Read(AssetReader reader)
		{
			Time = reader.ReadSingle();
			Curves = ReadAssetArray(reader);
		}

		public float Time { get; set; }
		public StreamedCurveKey[] Curves { get; set; } = Array.Empty<StreamedCurveKey>();

		private static StreamedCurveKey[] ReadAssetArray(AssetReader reader)
		{
			int count = reader.ReadInt32();
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), $"Cannot be negative: {count}");
			}

			StreamedCurveKey[] array = count == 0 ? Array.Empty<StreamedCurveKey>() : new StreamedCurveKey[count];
			for (int i = 0; i < count; i++)
			{
				StreamedCurveKey instance = new();
				instance.Read(reader);
				array[i] = instance;
			}
			if (reader.IsAlignArray)
			{
				reader.AlignStream();
			}
			return array;
		}
	}
}
