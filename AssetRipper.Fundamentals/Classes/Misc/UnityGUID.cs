using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles.IO;
using AssetRipper.Core.Project;
using AssetRipper.IO.Endian;
using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace AssetRipper.Core.Classes.Misc
{
	public record struct UnityGUID : IAsset, ISerializedReadable, ISerializedWritable
	{
		public UnityGUID(Guid guid) : this(ConvertSystemOrUnityBytes(guid.ToByteArray())) { }

		public UnityGUID(ReadOnlySpan<byte> guidData)
		{
			Data0 = BinaryPrimitives.ReadUInt32LittleEndian(guidData.Slice(0, sizeof(uint)));
			Data1 = BinaryPrimitives.ReadUInt32LittleEndian(guidData.Slice(4, sizeof(uint)));
			Data2 = BinaryPrimitives.ReadUInt32LittleEndian(guidData.Slice(8, sizeof(uint)));
			Data3 = BinaryPrimitives.ReadUInt32LittleEndian(guidData.Slice(12, sizeof(uint)));
		}

		public UnityGUID(uint data0, uint data1, uint data2, uint data3)
		{
			Data0 = data0;
			Data1 = data1;
			Data2 = data2;
			Data3 = data3;
		}

		public static UnityGUID NewGuid() => new UnityGUID(Guid.NewGuid().ToByteArray());

		public static explicit operator UnityGUID(Guid systemGuid) => new UnityGUID(systemGuid);

		public static explicit operator Guid(UnityGUID unityGuid) => new Guid(ConvertSystemOrUnityBytes(unityGuid.ToByteArray()));

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

		public YamlNode ExportYaml(IExportContainer container) => new YamlScalarNode(ToString());

		public byte[] ToByteArray()
		{
			byte[] result = new byte[16];
			BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(0, 4), Data0);
			BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(4, 4), Data1);
			BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(8, 4), Data2);
			BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(12, 4), Data3);
			return result;
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

		private static void Append(StringBuilder sb, uint value)
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
			{
				throw new ArgumentNullException(nameof(originalBytes));
			}

			if (originalBytes.Length != 16)
			{
				throw new ArgumentException($"Invalid length: {originalBytes.Length}", nameof(originalBytes));
			}

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

		public static UnityGUID Parse(string guidString) => new UnityGUID(Guid.Parse(guidString));

		/// <summary>
		/// Make a guid by MD5 hashing a string
		/// </summary>
		/// <remarks>
		/// The returned guid is most likely not "valid" by official standards. However, Unity doesn't seem to care.
		/// </remarks>
		/// <param name="str">Input string. Can be any length</param>
		/// <returns>A stable guid corresponding to the input string</returns>
		public static UnityGUID Md5Hash(string str) => Md5Hash(Encoding.UTF8.GetBytes(str));

		/// <summary>
		/// Make a guid by MD5 hashing some input data
		/// </summary>
		/// <remarks>
		/// The returned guid is most likely not "valid" by official standards. However, Unity doesn't seem to care.
		/// </remarks>
		/// <param name="inputBytes">Input byte array. Can be any length</param>
		/// <returns>A stable guid corresponding to the input bytes</returns>
		public static UnityGUID Md5Hash(byte[] inputBytes)
		{
			byte[] hashBytes = MD5.HashData(inputBytes);
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
		public static UnityGUID MissingReference { get; } = new UnityGUID(0xD0000000, 0x1FEEBDAE, 0x00FDAED5, 0x0000000D);

		public static UnityGUID Zero => default;

		[ThreadStatic]
		private static StringBuilder? s_sb = null;

		private static StringBuilder GetStringBuilder()
		{
			s_sb ??= new StringBuilder(32, 32);
			return s_sb;
		}
	}
}
