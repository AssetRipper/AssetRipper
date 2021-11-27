using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using System;
using System.IO;
using System.Text;

namespace AssetRipper.Core.Classes
{
	public class UnknownObject : UnityObjectBase
	{
		public byte[] Data { get; private set; } = Array.Empty<byte>();

		public UnknownObject(LayoutInfo layout) : base(layout) { }
		public UnknownObject(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			if(AssetInfo.ByteSize > 0)
				Data = reader.ReadBytes(AssetInfo.ByteSize);
		}

		public override void Write(AssetWriter writer)
		{
			writer.Write(Data);
		}

		public override string ExportExtension => "bytes";

		public override string ExportPath => Path.Combine("AssetRipper", "UnknownAssets", ClassID.ToString());

		public string ToFormattedHex()
		{
			StringBuilder sb = new StringBuilder();
			int count = 0;
			foreach(byte b in Data)
			{
				sb.Append(b.ToString("X2"));
				count++;
				if(count >= 16)
				{
					sb.AppendLine();
					count = 0;
				}
				else if(count % 4 == 0)
				{
					sb.Append('\t');
				}
				else
				{
					sb.Append(' ');
				}
			}
			return sb.ToString();
		}
	}
}
