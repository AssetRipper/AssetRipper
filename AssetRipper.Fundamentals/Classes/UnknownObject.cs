using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using System.IO;

namespace AssetRipper.Core.Classes
{
	public class UnknownObject : UnityObjectBase
	{
		public byte[] RawData { get; private set; } = Array.Empty<byte>();

		public UnknownObject(AssetInfo assetInfo) : base(assetInfo) { }

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

		public override string ExportExtension => "unknown";

		public override string ExportPath => Path.Combine("AssetRipper", "UnknownAssets", ClassID.ToString());
	}
}
