using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AudioMixers
{
#warning TODO: not implemented
	public struct AudioMixerConstant : IAssetReadable, IYAMLExportable
	{
		/*private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}*/

		public void Read(AssetStream stream)
		{
			m_groups = stream.ReadArray<GroupConstant>();
			m_groupGUIDs = stream.ReadArray<UtinyGUID>();
			m_effects = stream.ReadArray<EffectConstant>();
			m_effectGUIDs = stream.ReadArray<UtinyGUID>();
			NumSideChainBuffers = stream.ReadUInt32();
			m_snapshots = stream.ReadArray<SnapshotConstant>();
			m_snapshotGUIDs = stream.ReadArray<UtinyGUID>();
			//m_groupNameBuffer = stream.ReadArray<char>();
			stream.AlignStream(AlignType.Align4);
			
			//m_snapshotNameBuffer = stream.ReadArray<char>();
			stream.AlignStream(AlignType.Align4);
			
			//m_pluginEffectNameBuffer = stream.ReadArray<char>();
			stream.AlignStream(AlignType.Align4);
			
			m_exposedParameterNames = stream.ReadUInt32Array();
			m_exposedParameterIndices = stream.ReadUInt32Array();
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			//node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("groups", Groups.ExportYAML(exporter));
			node.Add("groupGUIDs", GroupGUIDs.ExportYAML(exporter));
			node.Add("effects", Effects.ExportYAML(exporter));
			node.Add("effectGUIDs", EffectGUIDs.ExportYAML(exporter));
			node.Add("numSideChainBuffers", NumSideChainBuffers);
			node.Add("snapshots", Snapshots.ExportYAML(exporter));
			node.Add("snapshotGUIDs", SnapshotGUIDs.ExportYAML(exporter));
			//node.Add("groupNameBuffer", GroupNameBuffer.ExportYAML(exporter));
			//node.Add("snapshotNameBuffer", SnapshotNameBuffer.ExportYAML(exporter));
			//node.Add("pluginEffectNameBuffer", PluginEffectNameBuffer.ExportYAML(exporter));
			node.Add("exposedParameterNames", ExposedParameterNames.ExportYAML(true));
			node.Add("exposedParameterIndices", ExposedParameterIndices.ExportYAML(true));
			return node;
		}

		public IReadOnlyList<GroupConstant> Groups => m_groups;
		public IReadOnlyList<UtinyGUID> GroupGUIDs => m_groupGUIDs;
		public IReadOnlyList<EffectConstant> Effects => m_effects;
		public IReadOnlyList<UtinyGUID> EffectGUIDs => m_effectGUIDs;
		public uint NumSideChainBuffers { get; private set; }
		public IReadOnlyList<SnapshotConstant> Snapshots => m_snapshots;
		public IReadOnlyList<UtinyGUID> SnapshotGUIDs => m_snapshotGUIDs;
		public IReadOnlyList<char> GroupNameBuffer => m_groupNameBuffer;
		public IReadOnlyList<char> SnapshotNameBuffer => m_snapshotNameBuffer;
		public IReadOnlyList<char> PluginEffectNameBuffer => m_pluginEffectNameBuffer;
		public IReadOnlyList<uint> ExposedParameterNames => m_exposedParameterNames;
		public IReadOnlyList<uint> ExposedParameterIndices => m_exposedParameterIndices;

		private GroupConstant[] m_groups;
		private UtinyGUID[] m_groupGUIDs;
		private EffectConstant[] m_effects;
		private UtinyGUID[] m_effectGUIDs;
		private SnapshotConstant[] m_snapshots;
		private UtinyGUID[] m_snapshotGUIDs;
		private char[] m_groupNameBuffer;
		private char[] m_snapshotNameBuffer;
		private char[] m_pluginEffectNameBuffer;
		private uint[] m_exposedParameterNames;
		private uint[] m_exposedParameterIndices;
	}
}
