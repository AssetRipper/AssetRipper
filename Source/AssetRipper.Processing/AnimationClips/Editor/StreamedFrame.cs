using AssetRipper.Assets.IO.Reading;

namespace AssetRipper.Processing.AnimationClips.Editor
{
	public sealed class StreamedFrame : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			Time = reader.ReadSingle();
			Curves = ReadAssetArray<StreamedCurveKey>(reader);
		}

		public float Time { get; set; }
		public StreamedCurveKey[] Curves { get; set; } = Array.Empty<StreamedCurveKey>();

		private static T[] ReadAssetArray<T>(AssetReader reader) where T : IAssetReadable, new()
		{
			int count = reader.ReadInt32();
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), $"Cannot be negative: {count}");
			}

			T[] array = count == 0 ? Array.Empty<T>() : new T[count];
			for (int i = 0; i < count; i++)
			{
				T instance = new T();
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
