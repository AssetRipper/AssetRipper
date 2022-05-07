using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using System;
using System.IO;

namespace AssetRipper.Core.Classes
{
	public class UnknownObject : UnityObjectBase
	{
		public byte[] RawData { get; private set; } = Array.Empty<byte>();

		public UnknownObject(LayoutInfo layout) : base(layout) { }
		public UnknownObject(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			if (AssetInfo.ByteSize > 0)
				RawData = reader.ReadBytes(AssetInfo.ByteSize);
		}

		public override void Write(AssetWriter writer)
		{
			writer.Write(RawData);
		}

		public override string ExportExtension => "unknown";

		public override string ExportPath => Path.Combine("AssetRipper", "UnknownAssets", ClassID.ToString());
	}
}
