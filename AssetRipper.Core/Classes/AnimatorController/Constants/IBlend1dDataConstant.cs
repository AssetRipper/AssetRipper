using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.AnimatorController.Constants
{
	public interface IBlend1dDataConstant : IAsset
	{
		float[] ChildThresholdArray { get; set; }
	}
}
