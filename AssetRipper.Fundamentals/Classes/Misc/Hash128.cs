using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Files.BundleFile.IO;
using AssetRipper.Core.Parser.Files.SerializedFiles.IO;
using AssetRipper.Core.Project;
using AssetRipper.IO.Endian;
using AssetRipper.Yaml;
using System.Diagnostics.CodeAnalysis;

namespace AssetRipper.Core.Classes.Misc
{
	public sealed class Hash128 : IAsset, ISerializedReadable, ISerializedWritable, IBundleReadable, IEquatable<Hash128>
	{
		public Hash128() { }

		public Hash128(uint v) : this(v, 0, 0, 0) { }

		public Hash128(uint v0, uint v1, uint v2, uint v3)
		{
			Data0 = v0;
			Data1 = v1;
			Data2 = v2;
			Data3 = v3;
		}

		public Hash128(byte[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			if (data.Length != 16)
			{
				throw new ArgumentException("Length must be exactly 16", nameof(data));
			}

			Data0 = BitConverter.ToUInt32(data, 0);
			Data1 = BitConverter.ToUInt32(data, 4);
			Data2 = BitConverter.ToUInt32(data, 8);
			Data3 = BitConverter.ToUInt32(data, 12);
		}

		public static int ToSerializedVersion(UnityVersion version)
		{
			if (version.IsGreaterEqual(5))
			{
				return 2;
			}
			return 1;
		}

		public void Read(BundleReader reader)
		{
			Read((EndianReader)reader);
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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			if (ToSerializedVersion(container.Version) > 1)
			{
				node.Add(HashName, $"{Data0:x8}{Data1:x8}{Data2:x8}{Data3:x8}");
			}
			else
			{
				node.Add(Bytes0Name, (byte)((Data0 & 0x000000FF) >> 0));
				node.Add(Bytes1Name, (byte)((Data0 & 0x0000FF00) >> 8));
				node.Add(Bytes2Name, (byte)((Data0 & 0x00FF0000) >> 16));
				node.Add(Bytes3Name, (byte)((Data0 & 0xFF000000) >> 24));
				node.Add(Bytes4Name, (byte)((Data1 & 0x000000FF) >> 0));
				node.Add(Bytes5Name, (byte)((Data1 & 0x0000FF00) >> 8));
				node.Add(Bytes6Name, (byte)((Data1 & 0x00FF0000) >> 16));
				node.Add(Bytes7Name, (byte)((Data1 & 0xFF000000) >> 24));
				node.Add(Bytes8Name, (byte)((Data2 & 0x000000FF) >> 0));
				node.Add(Bytes9Name, (byte)((Data2 & 0x0000FF00) >> 8));
				node.Add(Bytes10Name, (byte)((Data2 & 0x00FF0000) >> 16));
				node.Add(Bytes11Name, (byte)((Data2 & 0xFF000000) >> 24));
				node.Add(Bytes12Name, (byte)((Data3 & 0x000000FF) >> 0));
				node.Add(Bytes13Name, (byte)((Data3 & 0x0000FF00) >> 8));
				node.Add(Bytes14Name, (byte)((Data3 & 0x00FF0000) >> 16));
				node.Add(Bytes15Name, (byte)((Data3 & 0xFF000000) >> 24));
			}
			return node;
		}

		public override bool Equals([NotNullWhen(true)] object? obj)
		{
			if (obj is Hash128 hash)
			{
				return Equals(hash);
			}
			else
			{
				return false;
			}
		}

		public bool Equals(Hash128? other)
		{
			return other is not null && this.Data0 == other.Data0 && this.Data1 == other.Data1 && this.Data2 == other.Data2 && this.Data3 == other.Data3;
		}

		public static bool operator ==(Hash128 left, Hash128 right) => left.Equals(right);

		public static bool operator !=(Hash128 left, Hash128 right) => !left.Equals(right);

		public override int GetHashCode()
		{
			return HashCode.Combine(Data0, Data1, Data2, Data3);
		}

		public override string ToString()
		{
			return new UnityGUID(Data0, Data1, Data2, Data3).ToString();
		}

		public uint Data0 { get; set; }
		public uint Data1 { get; set; }
		public uint Data2 { get; set; }
		public uint Data3 { get; set; }

		public const string Bytes0Name = "bytes[0]";
		public const string Bytes1Name = "bytes[1]";
		public const string Bytes2Name = "bytes[2]";
		public const string Bytes3Name = "bytes[3]";
		public const string Bytes4Name = "bytes[4]";
		public const string Bytes5Name = "bytes[5]";
		public const string Bytes6Name = "bytes[6]";
		public const string Bytes7Name = "bytes[7]";
		public const string Bytes8Name = "bytes[8]";
		public const string Bytes9Name = "bytes[9]";
		public const string Bytes10Name = "bytes[10]";
		public const string Bytes11Name = "bytes[11]";
		public const string Bytes12Name = "bytes[12]";
		public const string Bytes13Name = "bytes[13]";
		public const string Bytes14Name = "bytes[14]";
		public const string Bytes15Name = "bytes[15]";
		public const string HashName = "Hash";
	}
}
