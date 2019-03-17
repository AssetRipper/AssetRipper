using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AudioMixers
{
#warning TODO: not implemented
	public struct EffectConstant : IAssetReadable, IYAMLExportable
	{
		/*private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}*/

		public void Read(AssetReader reader)
		{
			Type = reader.ReadInt32();
			GroupConstantIndex = reader.ReadUInt32();
			SendTargetEffectIndex = reader.ReadUInt32();
			WetMixLevelIndex = reader.ReadUInt32();
			PrevEffectIndex = reader.ReadUInt32();
			Bypass = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
			
			m_parameterIndices = reader.ReadUInt32Array();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			//node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("type", Type);
			node.Add("groupConstantIndex", GroupConstantIndex);
			node.Add("sendTargetEffectIndex", SendTargetEffectIndex);
			node.Add("wetMixLevelIndex", WetMixLevelIndex);
			node.Add("prevEffectIndex", PrevEffectIndex);
			node.Add("bypass", Bypass);
			node.Add("parameterIndices", ParameterIndices.ExportYAML(true));
			return node;
		}

		public int Type { get; private set; }
		public uint GroupConstantIndex { get; private set; }
		public uint SendTargetEffectIndex { get; private set; }
		public uint WetMixLevelIndex { get; private set; }
		public uint PrevEffectIndex { get; private set; }
		public bool Bypass { get; private set; }
		public IReadOnlyList<uint> ParameterIndices => m_parameterIndices;

		private uint[] m_parameterIndices;
	}
}
