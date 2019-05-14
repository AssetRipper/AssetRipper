using System.Collections.Generic;
using uTinyRipper.Assembly;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public struct LayerMask : IScriptStructure
	{
		private static int GetSerializedVersion(Version version)
		{
			// TODO:
			return 2;
		}

		public IScriptStructure CreateDuplicate()
		{
			return new LayerMask();
		}

		public void Read(AssetReader reader)
		{
			Bits = reader.ReadUInt32();
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
