using System;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public struct UtinyGUID : IAssetReadable, IYAMLExportable
	{
		static UtinyGUID()
		{
			MissingReference = new UtinyGUID
			{
				Data0 = 0xD0000000,
				Data1 = 0x5DEADF00,
				Data2 = 0xEADBEEF1,
				Data3 = 0x0000000D
			};
		}
		
		public UtinyGUID(Guid guid):
			this(guid.ToByteArray())
		{
		}

		public UtinyGUID(byte[] guidData)
		{
			Data0 = BitConverter.ToUInt32(guidData, 0);
			Data1 = BitConverter.ToUInt32(guidData, 4);
			Data2 = BitConverter.ToUInt32(guidData, 8);
			Data3 = BitConverter.ToUInt32(guidData, 12);
		}

		public void Read(AssetStream stream)
		{
			Data0 = stream.ReadUInt32();
			Data1 = stream.ReadUInt32();
			Data2 = stream.ReadUInt32();
			Data3 = stream.ReadUInt32();
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			return new YAMLScalarNode(ToString());
		}

		public override string ToString() => $"{Data3:x8}{Data2:x8}{Data1:x8}{Data0:x8}";

		public bool IsZero => Data0 == 0 && Data1 == 0 && Data2 == 0 && Data3 == 0;

		public uint Data0 { get; private set; }
		public uint Data1 { get; private set; }
		public uint Data2 { get; private set; }
		public uint Data3 { get; private set; }

		public static readonly UtinyGUID MissingReference;
	}
}
