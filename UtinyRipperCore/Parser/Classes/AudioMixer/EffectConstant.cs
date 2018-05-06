using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AudioMixers
{
#warning TODO: not implemented
	public struct EffectConstant : IAssetReadable, IYAMLExportable
	{
		/*private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}*/

		public void Read(AssetStream stream)
		{
			Type = stream.ReadInt32();
			GroupConstantIndex = stream.ReadUInt32();
			SendTargetEffectIndex = stream.ReadUInt32();
			WetMixLevelIndex = stream.ReadUInt32();
			PrevEffectIndex = stream.ReadUInt32();
			Bypass = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
			
			m_parameterIndices = stream.ReadUInt32Array();
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
