using AssetRipper.Parser.IO.Asset.Reader;

namespace AssetRipper.Parser.Classes.AnimationClip.Editor
{
	public struct StreamedFrame : IAssetReadable
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
