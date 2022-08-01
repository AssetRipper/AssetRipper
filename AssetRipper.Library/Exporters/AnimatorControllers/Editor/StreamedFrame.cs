using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Library.Exporters.AnimatorControllers.Editor
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
