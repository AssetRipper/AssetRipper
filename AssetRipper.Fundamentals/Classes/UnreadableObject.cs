using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using System.IO;

namespace AssetRipper.Core.Classes
{
	public class UnreadableObject : UnityObjectBase, IHasNameString, IHasRawData
	{
		public byte[] RawData { get; private set; } = Array.Empty<byte>();
		public string NameString { get; set; } = "";

		public UnreadableObject(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			throw new NotSupportedException();
		}

		public void Read(AssetReader reader, int byteSize)
		{
			if (byteSize > 0)
			{
				RawData = reader.ReadBytes(byteSize);
			}
		}

		public override void Write(AssetWriter writer)
		{
			writer.Write(RawData);
		}

		public override string ExportExtension => "unreadable";

		public override string ExportPath => Path.Combine("AssetRipper", "UnreadableAssets", ClassID.ToString());
	}
}
