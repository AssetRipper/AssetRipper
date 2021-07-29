using AssetRipper.IO.Asset;

namespace AssetRipper.Classes.AnimationClip.Editor
{
	public class StreamedFrame : IAssetReadable
	{
		public StreamedFrame() { }

		public void Read(AssetReader reader)
		{
			Time = reader.ReadSingle();
			Curves = reader.ReadAssetArray<StreamedCurveKey>();
		}

		public float Time { get; set; }
		public StreamedCurveKey[] Curves { get; set; }
	}
}
