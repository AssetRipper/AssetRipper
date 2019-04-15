using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public struct Hash128 : IAssetReadable, ISerializedFileReadable, IYAMLExportable
	{
		public Hash128(uint v):
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

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

			if (version.IsGreaterEqual(5))
			{
				return 2;
			}
			return 1;
		}

		public void Read(EndianReader reader)
		{
			Data0 = reader.ReadUInt32();
			Data1 = reader.ReadUInt32();
			Data2 = reader.ReadUInt32();
			Data3 = reader.ReadUInt32();
		}
		
		public void Read(AssetReader reader)
		{
			Read((EndianReader)reader);
		}

		public void Read(SerializedFileReader reader)
		{
			Read((EndianReader)reader);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			if (GetSerializedVersion(container.Version) == 1)
			{
				node.Add("bytes[0]", (byte)((Data0 & 0x000000FF) >> 0));
				node.Add("bytes[1]", (byte)((Data0 & 0x0000FF00) >> 8));
				node.Add("bytes[2]", (byte)((Data0 & 0x00FF0000) >> 16));
				node.Add("bytes[3]", (byte)((Data0 & 0xFF000000) >> 24));
				node.Add("bytes[4]", (byte)((Data1 & 0x000000FF) >> 0));
				node.Add("bytes[5]", (byte)((Data1 & 0x0000FF00) >> 8));
				node.Add("bytes[6]", (byte)((Data1 & 0x00FF0000) >> 16));
				node.Add("bytes[7]", (byte)((Data1 & 0xFF000000) >> 24));
				node.Add("bytes[8]", (byte)((Data2 & 0x000000FF) >> 0));
				node.Add("bytes[9]", (byte)((Data2 & 0x0000FF00) >> 8));
				node.Add("bytes[10]", (byte)((Data2 & 0x00FF0000) >> 16));
				node.Add("bytes[11]", (byte)((Data2 & 0xFF000000) >> 24));
				node.Add("bytes[12]", (byte)((Data3 & 0x000000FF) >> 0));
				node.Add("bytes[13]", (byte)((Data3 & 0x0000FF00) >> 8));
				node.Add("bytes[14]", (byte)((Data3 & 0x00FF0000) >> 16));
				node.Add("bytes[15]", (byte)((Data3 & 0xFF000000) >> 24));
			}
			else
			{
				node.Add("Hash", $"{Data0:x8}{Data1:x8}{Data2:x8}{Data3:x8}");
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

		public uint Data0 { get; private set; }
		public uint Data1 { get; private set; }
		public uint Data2 { get; private set; }
		public uint Data3 { get; private set; }
	}
}
