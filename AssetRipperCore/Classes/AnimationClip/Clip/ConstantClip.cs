using AssetRipper.IO.Asset;

namespace AssetRipper.Classes.AnimationClip.Clip
{
	public struct ConstantClip : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			Constants = reader.ReadSingleArray();
		}

		public bool IsSet => Constants.Length > 0;

		public float[] Constants { get; set; }
	}
}
