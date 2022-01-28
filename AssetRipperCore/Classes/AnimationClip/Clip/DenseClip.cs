using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.AnimationClip.Clip
{
	public sealed class DenseClip : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			FrameCount = reader.ReadInt32();
			CurveCount = (int)reader.ReadUInt32();
			SampleRate = reader.ReadSingle();
			BeginTime = reader.ReadSingle();

			SampleArray = reader.ReadSingleArray();
		}

		public bool IsSet => SampleArray.Length > 0;

		public int FrameCount { get; set; }
		public int CurveCount { get; set; }
		public float SampleRate { get; set; }
		public float BeginTime { get; set; }
		public float[] SampleArray { get; set; }
	}
}
