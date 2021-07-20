using AssetRipper.Converters.Project;
using AssetRipper.Parser.Classes.Utils.Extensions;
using AssetRipper.Parser.IO.Asset;
using AssetRipper.Parser.IO.Asset.Reader;
using AssetRipper.Parser.IO.Asset.Writer;
using AssetRipper.Parser.IO.Extensions;
using AssetRipper.YAML;
using AssetRipper.YAML.Extensions;
using System;
using Version = AssetRipper.Parser.Files.File.Version.Version;

namespace AssetRipper.Parser.Classes.Misc
{
	public struct MdFour : IAsset
	{
		public static int ToSerializedVersion(Version version)
		{
			// unknown version
			return 2;
		}

		public void Read(AssetReader reader)
		{
			Md4Hash = reader.ReadByteArray();
			reader.AlignStream();
		}

		public void Write(AssetWriter writer)
		{
			Md4Hash.Write(writer);
			writer.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.Version));
			node.Add(Md4HashName, Md4Hash.ExportYAML());
			return node;
		}

		public override string ToString()
		{
			if (Md4Hash == null)
			{
				return base.ToString();
			}
			uint data0 = BitConverter.ToUInt32(Md4Hash, 0);
			uint data1 = BitConverter.ToUInt32(Md4Hash, 4);
			uint data2 = BitConverter.ToUInt32(Md4Hash, 8);
			uint data3 = BitConverter.ToUInt32(Md4Hash, 12);
			return $"{data3:x8}{data2:x8}{data1:x8}{data0:x8}";
		}

		public byte[] Md4Hash { get; set; }

		public const string Md4HashName = "md4 hash";
	}
}
