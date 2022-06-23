using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.Mesh
{
	public interface IStreamInfoLegacy : IAsset
	{
		uint ChannelMask { get; set; }
		uint Offset { get; set; }
		uint Stride { get; set; }
		uint Align { get; set; }
		byte DividerOp { get; set; }
		ushort Frequency { get; set; }
	}
}
