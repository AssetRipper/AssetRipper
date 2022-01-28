using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.AudioMixer
{
	public sealed class GroupConnection : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			sourceGroupIndex = reader.ReadUInt32();
			targetGroupIndex = reader.ReadUInt32();
			sendEffectIndex = reader.ReadUInt32();
		}

		public uint sourceGroupIndex;

		public uint targetGroupIndex;

		public uint sendEffectIndex;
	}
}
