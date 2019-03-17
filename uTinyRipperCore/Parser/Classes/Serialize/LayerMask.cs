using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public struct LayerMask : IScriptStructure
	{
		public LayerMask(LayerMask copy)
		{
			Bits = copy.Bits;
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

#warning TODO: unknown
			return 2;
		}

		public IScriptStructure CreateCopy()
		{
			return new LayerMask(this);
		}

		public void Read(AssetReader reader)
		{
			Bits = reader.ReadUInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_Bits", Bits);
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield break;
		}

		public IScriptStructure Base => null;
		public string Namespace => ScriptType.UnityEngineName;
		public string Name => ScriptType.LayerMaskName;

		public uint Bits { get; private set; }
	}
}
