using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.AnimatorController.State
{
	public sealed class StateRange : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			StartIndex = (int)reader.ReadUInt32();
			Count = (int)reader.ReadUInt32();
		}

		public int StartIndex { get; set; }
		public int Count { get; set; }
	}
}
