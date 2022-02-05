using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Math.Vectors
{
	public interface IQuaternionf : IAsset
	{
		float X { get; set; }
		float Y { get; set; }
		float Z { get; set; }
		float W { get; set; }
	}
}
