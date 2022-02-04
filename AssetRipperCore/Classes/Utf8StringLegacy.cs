using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes
{
	public sealed class Utf8StringLegacy : Utf8StringBase
	{
		public override byte[] Data { get; set; }

		public override void ReadRelease(AssetReader reader)
		{
			Data = reader.ReadByteArray();
		}

		public override void ReadEditor(AssetReader reader)
		{
			Data = reader.ReadByteArray();
		}
	}
}
