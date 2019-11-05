using uTinyRipper.Converters;
using uTinyRipper.SerializedFiles;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public struct LayerMask : IAsset
	{
		public static int ToSerializedVersion(Version version)
		{
			// Bits size has been changed to 32
			if (version.IsGreaterEqual(2))
			{
				return 2;
			}
			return 1;
		}

		private static bool Is32Bits(Version version) => version.IsGreaterEqual(2);

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			int version = ToSerializedVersion(context.Version);
			context.AddNode(TypeTreeUtils.BitFieldName, name, 0, version);
			context.BeginChildren();
			if (Is32Bits(context.Version))
			{
				context.AddUInt32(BitsName);
			}
			else
			{
				context.AddUInt16(BitsName);
			}
			context.EndChildren();
		}

		public void Read(AssetReader reader)
		{
			Bits = Is32Bits(reader.Version) ? reader.ReadUInt32() : reader.ReadUInt16();
		}

		public void Write(AssetWriter writer)
		{
			if (Is32Bits(writer.Version))
			{
				writer.Write(Bits);
			}
			else
			{
				writer.Write((ushort)Bits);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(BitsName, Bits);
			return node;
		}

		public uint Bits { get; set; }

		public const string BitsName = "m_Bits";
	}
}
