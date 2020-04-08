using System;
using System.Text;
using uTinyRipper.Converters;
using uTinyRipper.SerializedFiles;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Misc
{
	public struct UnityGUID : IAsset, ISerializedReadable, ISerializedWritable
	{
		public UnityGUID(Guid guid) :
			this(guid.ToByteArray())
		{
		}

		public UnityGUID(byte[] guidData)
		{
			Data0 = BitConverter.ToUInt32(guidData, 0);
			Data1 = BitConverter.ToUInt32(guidData, 4);
			Data2 = BitConverter.ToUInt32(guidData, 8);
			Data3 = BitConverter.ToUInt32(guidData, 12);
		}

		public UnityGUID(uint dword0, uint dword1, uint dword2, uint dword3)
		{
			Data0 = dword0;
			Data1 = dword1;
			Data2 = dword2;
			Data3 = dword3;
		}

		public static bool operator ==(UnityGUID left, UnityGUID right)
		{
			return left.Data0 == right.Data0 && left.Data1 == right.Data1 && left.Data2 == right.Data2 && left.Data3 == right.Data3;
		}

		public static bool operator !=(UnityGUID left, UnityGUID right)
		{
			return left.Data0 != right.Data0 || left.Data1 != right.Data1 || left.Data2 != right.Data2 || left.Data3 != right.Data3;
		}

		public void Read(SerializedReader reader)
		{
			Read((EndianReader)reader);
		}

		public void Read(AssetReader reader)
		{
			Read((EndianReader)reader);
		}

		public void Read(EndianReader reader)
		{
			Data0 = reader.ReadUInt32();
			Data1 = reader.ReadUInt32();
			Data2 = reader.ReadUInt32();
			Data3 = reader.ReadUInt32();
		}

		public void Write(SerializedWriter writer)
		{
			Write((EndianWriter)writer);
		}

		public void Write(AssetWriter writer)
		{
			Write((EndianWriter)writer);
		}

		public void Write(EndianWriter writer)
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

		public override bool Equals(object obj)
		{
			if (obj is UnityGUID guid)
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

		public override string ToString()
		{
			StringBuilder sb = GetStringBuilder();
			try
			{
				Append(sb, Data0);
				Append(sb, Data1);
				Append(sb, Data2);
				Append(sb, Data3);
				return sb.ToString();
			}
			finally
			{
				sb.Clear();
			}
		}

		private static StringBuilder GetStringBuilder()
		{
			if (s_sb == null)
			{
				s_sb = new StringBuilder(32, 32);
			}
			return s_sb;
		}

		private void Append(StringBuilder sb, uint value)
		{
			sb.Append(StringBuilderExtensions.ByteHexRepresentations[unchecked((int)(value << 4) & 0xF0) | unchecked((int)(value >> 4) & 0xF)]);
			sb.Append(StringBuilderExtensions.ByteHexRepresentations[unchecked((int)(value >> 4) & 0xF0) | unchecked((int)(value >> 12) & 0xF)]);
			sb.Append(StringBuilderExtensions.ByteHexRepresentations[unchecked((int)(value >> 12) & 0xF0) | unchecked((int)(value >> 20) & 0xF)]);
			sb.Append(StringBuilderExtensions.ByteHexRepresentations[unchecked((int)(value >> 20) & 0xF0) | unchecked((int)(value >> 28) & 0xF)]);
		}

		public bool IsZero => Data0 == 0 && Data1 == 0 && Data2 == 0 && Data3 == 0;

		public uint Data0 { get; set; }
		public uint Data1 { get; set; }
		public uint Data2 { get; set; }
		public uint Data3 { get; set; }

		public static readonly UnityGUID MissingReference = new UnityGUID(0xD0000000, 0x1FEEBDAE, 0x00FDAED5, 0x0000000D);

		[ThreadStatic]
		private static StringBuilder s_sb = null;
	}
}
