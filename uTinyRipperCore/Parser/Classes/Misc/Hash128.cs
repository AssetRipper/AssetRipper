using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;
using uTinyRipper.Converters;
using uTinyRipper.BundleFiles;

namespace uTinyRipper.Classes.Misc
{
	public struct Hash128 : IAsset, ISerializedReadable, ISerializedWritable, IBundleReadable
	{
		public Hash128(uint v) :
			this(v, 0, 0, 0)
		{
		}

		public Hash128(uint v0, uint v1, uint v2, uint v3)
		{
			Data0 = v0;
			Data1 = v1;
			Data2 = v2;
			Data3 = v3;
		}

		public static int ToSerializedVersion(Version version)
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

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
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

		public override int GetHashCode()
		{
			int hash = 311;
			unchecked
			{
				hash = hash + 709 * Data0.GetHashCode();
				hash = hash * 443 + Data1.GetHashCode();
				hash = hash * 269 + Data2.GetHashCode();
				hash = hash * 653 + Data3.GetHashCode();
			}
			return hash;
		}

		public override string ToString()
		{
			UnityGUID guid = new UnityGUID(Data0, Data1, Data2, Data3);
			return guid.ToString();
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
