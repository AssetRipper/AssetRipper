using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.AnimationClip.Clip
{
	public sealed class ConstantClip : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			Constants = reader.ReadSingleArray();
		}

		public bool IsSet => Constants.Length > 0;

		public float[] Constants { get; set; }
	}
}
