using AssetRipper.Core.Extensions;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.Parser.Files.SerializedFiles.IO;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace AssetRipper.Core.Classes.Misc
{
	public struct UnityGUID : IAsset, ISerializedReadable, ISerializedWritable, IEquatable<UnityGUID>
	{
		public UnityGUID(Guid guid) : this(ConvertSystemOrUnityBytes(guid.ToByteArray())) { }

		public UnityGUID(byte[] guidData)
		{
			Data0 = BinaryPrimitives.ReadUInt32LittleEndian(guidData.AsSpan(0, 4));
			Data1 = BinaryPrimitives.ReadUInt32LittleEndian(guidData.AsSpan(4, 4));
			Data2 = BinaryPrimitives.ReadUInt32LittleEndian(guidData.AsSpan(8, 4));
			Data3 = BinaryPrimitives.ReadUInt32LittleEndian(guidData.AsSpan(12, 4));
		}

		public UnityGUID(uint dword0, uint dword1, uint dword2, uint dword3)
		{
			Data0 = dword0;
			Data1 = dword1;
			Data2 = dword2;
			Data3 = dword3;
		}

		public static UnityGUID NewGuid() => new UnityGUID(Guid.NewGuid().ToByteArray());

		public static explicit operator UnityGUID(Guid systemGuid) => new UnityGUID(systemGuid);

		public static explicit operator Guid(UnityGUID unityGuid) => new Guid(ConvertSystemOrUnityBytes(unityGuid.ToByteArray()));

		public static bool operator ==(UnityGUID left, UnityGUID right)
		{
			return left.Data0 == right.Data0 && left.Data1 == right.Data1 && left.Data2 == right.Data2 && left.Data3 == right.Data3;
		}

		public static bool operator !=(UnityGUID left, UnityGUID right)
		{
			return left.Data0 != right.Data0 || left.Data1 != right.Data1 || left.Data2 != right.Data2 || left.Data3 != right.Data3;
		}

		public void Read(SerializedReader reader) => Read((EndianReader)reader);
		public void Read(AssetReader reader) => Read((EndianReader)reader);
		public void Read(EndianReader reader)
		{
			Data0 = reader.ReadUInt32();
			Data1 = reader.ReadUInt32();
			Data2 = reader.ReadUInt32();
			Data3 = reader.ReadUInt32();
		}

		public void Write(SerializedWriter writer) => Write((EndianWriter)writer);
		public void Write(AssetWriter writer) => Write((EndianWriter)writer);
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

		public byte[] ToByteArray()
		{
			byte[] result = new byte[16];
			BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(0, 4), Data0);
			BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(4, 4), Data1);
			BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(8, 4), Data2);
			BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(12, 4), Data3);
			return result;
		}

		public override bool Equals(object obj)
		{
			if (obj is UnityGUID guid)
			{
				return this == guid;
			}
			return false;
		}

		public bool Equals(UnityGUID other) => this == other;

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

		/// <summary>
		/// Converts system bytes to unity bytes, or the reverse
		/// </summary>
		/// <param name="originalBytes">A 16 byte input array</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">Array is null</exception>
		/// <exception cref="ArgumentException">Array doesn't have 16 elements</exception>
		private static byte[] ConvertSystemOrUnityBytes(byte[] originalBytes)
		{
			if (originalBytes is null)
				throw new ArgumentNullException(nameof(originalBytes));
			if (originalBytes.Length != 16)
				throw new ArgumentException($"Invalid length: {originalBytes.Length}", nameof(originalBytes));

			byte[] newBytes = new byte[16];
			for (int i = 0; i < 4; i++)
			{
				newBytes[i] = originalBytes[3 - i];
			}
			newBytes[4] = originalBytes[5];
			newBytes[5] = originalBytes[4];
			newBytes[6] = originalBytes[7];
			newBytes[7] = originalBytes[6];
			for (int i = 8; i < 16; i++)
			{
				newBytes[i] = originalBytes[i];
			}
			for (int i = 0; i < 16; i++)
			{
				//AB becomes BA
				byte value = newBytes[i];
				newBytes[i] = (byte)(unchecked((int)(value << 4) & 0xF0) | unchecked((int)(value >> 4) & 0xF));
			}

			return newBytes;
		}

		public static UnityGUID Parse(string guidString)
		{
			return new UnityGUID(Guid.Parse(guidString));
		}

		public static UnityGUID Md5Hash(string str) => Md5Hash(Encoding.UTF8.GetBytes(str));
		public static UnityGUID Md5Hash(byte[] inputBytes)
		{
			const byte VersionMask = 0xF0;
			const byte Md5GuidVersion = 0x30;
			const byte ClockSeqHiAndReservedMask = 0xC0;
			const byte ClockSeqHiAndReservedValue = 0x80;

			byte[] hashBytes = MD5.HashData(inputBytes);

			// time_hi_and_version
			hashBytes[7] = (byte)((hashBytes[7] & ~VersionMask) | Md5GuidVersion);
			// clock_seq_hi_and_reserved
			hashBytes[8] = (byte)((hashBytes[8] & ~ClockSeqHiAndReservedMask) | ClockSeqHiAndReservedValue);

			return new UnityGUID(ConvertSystemOrUnityBytes(hashBytes));
		}

		public bool IsZero => Data0 == 0 && Data1 == 0 && Data2 == 0 && Data3 == 0;

		public uint Data0 { get; set; }
		public uint Data1 { get; set; }
		public uint Data2 { get; set; }
		public uint Data3 { get; set; }

		/// <summary>
		/// 0x0000000DEADBEEF15DEADF00D0000000
		/// </summary>
		public static readonly UnityGUID MissingReference = new UnityGUID(0xD0000000, 0x1FEEBDAE, 0x00FDAED5, 0x0000000D);

		[ThreadStatic]
		private static StringBuilder s_sb = null;
	}
}
