using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes
{
	public sealed class Utf8StringLegacy : Utf8StringBase
	{
		public override byte[] Data { get; set; }

		public Utf8StringLegacy() { }

		public Utf8StringLegacy(string content)
		{
			String = content;
		}

		public override void ReadRelease(AssetReader reader)
		{
			Data = reader.ReadByteArray();
			reader.AlignStream();
		}

		public override void ReadEditor(AssetReader reader)
		{
			Data = reader.ReadByteArray();
			reader.AlignStream();
		}
	}
}
