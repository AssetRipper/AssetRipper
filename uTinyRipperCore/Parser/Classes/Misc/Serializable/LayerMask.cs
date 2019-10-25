using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.Game.Assembly;

namespace uTinyRipper.Classes
{
	public struct LayerMask : ISerializableStructure
	{
		private static int GetSerializedVersion(Version version)
		{
			// TODO:
			return 2;
		}

		public ISerializableStructure CreateDuplicate()
		{
			return new LayerMask();
		}

		public void Read(AssetReader reader)
		{
			Bits = reader.ReadUInt32();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(Bits);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add("m_Bits", Bits);
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield break;
		}

		public uint Bits { get; private set; }
	}
}
