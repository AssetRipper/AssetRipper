using AssetRipper.Assets.IO.Reading;
using AssetRipper.Import.IO.Extensions;

namespace AssetRipper.Processing.AnimationClips.Editor
{
	public sealed class StreamedFrame : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			Time = reader.ReadSingle();
			Curves = reader.ReadAssetArray<StreamedCurveKey>();
		}

		public float Time { get; set; }
		public StreamedCurveKey[] Curves { get; set; } = Array.Empty<StreamedCurveKey>();
	}
}
