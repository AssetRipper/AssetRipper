using AssetRipper.Core.IO;

namespace AssetRipper.Core.Reading.Classes
{
	public class AssetInfo
	{
		public int preloadIndex;
		public int preloadSize;
		public PPtr<Classes.Object> asset;

		public AssetInfo(ObjectReader reader)
		{
			preloadIndex = reader.ReadInt32();
			preloadSize = reader.ReadInt32();
			asset = new PPtr<Classes.Object>(reader);
		}
	}
}
