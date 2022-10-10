using AssetRipper.Assets.IO;

namespace AssetRipper.Core.Math.Vectors
{
	public interface IVector3i : IAsset
	{
		int X { get; set; }
		int Y { get; set; }
		int Z { get; set; }
	}
}
