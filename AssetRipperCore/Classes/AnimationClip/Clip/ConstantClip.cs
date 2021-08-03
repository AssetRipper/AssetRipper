using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.AnimationClip.Clip
{
	public class ConstantClip : IAssetReadable
	{
		public ConstantClip() { }

		public void Read(AssetReader reader)
		{
			Constants = reader.ReadSingleArray();
		}

		public bool IsSet => Constants.Length > 0;

		public float[] Constants { get; set; }
	}
}
