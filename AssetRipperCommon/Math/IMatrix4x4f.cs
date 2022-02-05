using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Math
{
	public interface IMatrix4x4f : IAsset
	{
		float E00 { get; set; }
		float E01 { get; set; }
		float E02 { get; set; }
		float E03 { get; set; }
		float E10 { get; set; }
		float E11 { get; set; }
		float E12 { get; set; }
		float E13 { get; set; }
		float E20 { get; set; }
		float E21 { get; set; }
		float E22 { get; set; }
		float E23 { get; set; }
		float E30 { get; set; }
		float E31 { get; set; }
		float E32 { get; set; }
		float E33 { get; set; }
	}
}
