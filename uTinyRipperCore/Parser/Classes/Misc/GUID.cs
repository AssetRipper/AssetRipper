using System;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Misc
{
	public struct GUID : IAsset
	{
		public GUID(Guid guid) :
			this(guid.ToByteArray())
		{
		}

		public GUID(byte[] guidData)
		{
			Data0 = BitConverter.ToUInt32(guidData, 0);
			Data1 = BitConverter.ToUInt32(guidData, 4);
			Data2 = BitConverter.ToUInt32(guidData, 8);
			Data3 = BitConverter.ToUInt32(guidData, 12);
		}

		public GUID(uint dword0, uint dword1, uint dword2, uint dword3)
		{
			Data0 = dword0;
			Data1 = dword1;
			Data2 = dword2;
			Data3 = dword3;
		}

		public static bool operator ==(GUID left, GUID right)
		{
			return left.Data0 == right.Data0 && left.Data1 == right.Data1 && left.Data2 == right.Data2 && left.Data3 == right.Data3;
		}

		public static bool operator !=(GUID left, GUID right)
		{
			return left.Data0 != right.Data0 || left.Data1 != right.Data1 || left.Data2 != right.Data2 || left.Data3 != right.Data3;
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			context.AddNode(nameof(GUID), name);
			context.BeginChildren();
			context.AddUInt32(Data0Name);
			context.AddUInt32(Data1Name);
			context.AddUInt32(Data2Name);
			context.AddUInt32(Data3Name);
			context.EndChildren();
		}

		public void Read(AssetReader reader)
		{
			Data0 = reader.ReadUInt32();
			Data1 = reader.ReadUInt32();
			Data2 = reader.ReadUInt32();
			Data3 = reader.ReadUInt32();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(Data0);
			writer.Write(Data1);
			writer.Write(Data2);
			writer.Write(Data3);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			return new YAMLScalarNode(ToString());
		}

		public override string ToString() => $"{Data3:x8}{Data2:x8}{Data1:x8}{Data0:x8}";

		public override bool Equals(object obj)
		{
			if (obj is GUID guid)
			{
				return this == guid;
			}
			return false;
		}

		public override int GetHashCode()
		{
			int hash = 19;
			unchecked
			{
				hash = hash + 31 * Data0.GetHashCode();
				hash = hash * 479 + Data1.GetHashCode();
				hash = hash * 593 + Data2.GetHashCode();
				hash = hash * 347 + Data3.GetHashCode();
			}
			return hash;
		}

		public bool IsZero => Data0 == 0 && Data1 == 0 && Data2 == 0 && Data3 == 0;

		public uint Data0 { get; set; }
		public uint Data1 { get; set; }
		public uint Data2 { get; set; }
		public uint Data3 { get; set; }

		public static readonly GUID MissingReference = new GUID(0xD0000000, 0x5DEADF00, 0xEADBEEF1, 0x0000000D);

		public const string Data0Name = "data[0]";
		public const string Data1Name = "data[1]";
		public const string Data2Name = "data[2]";
		public const string Data3Name = "data[3]";
	}
}
