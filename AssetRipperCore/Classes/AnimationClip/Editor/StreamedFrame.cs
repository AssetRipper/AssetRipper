using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.AnimationClip.Editor
{
	public sealed class StreamedFrame : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			Time = reader.ReadSingle();
			Curves = reader.ReadAssetArray<StreamedCurveKey>();
		}

		public float Time { get; set; }
		public StreamedCurveKey[] Curves { get; set; }
	}
}
