using AssetRipper.Parser.IO.Asset.Reader;

namespace AssetRipper.Parser.Classes.AnimatorController.State
{
	public struct StateRange : IAssetReadable
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
