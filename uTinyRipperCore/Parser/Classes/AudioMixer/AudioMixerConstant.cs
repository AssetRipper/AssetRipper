using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AudioMixers
{
#warning TODO: not implemented
	public struct AudioMixerConstant : IAssetReadable, IYAMLExportable
	{
		/*private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}*/

		public void Read(AssetReader reader)
		{
			m_groups = reader.ReadAssetArray<GroupConstant>();
			m_groupGUIDs = reader.ReadAssetArray<EngineGUID>();
			m_effects = reader.ReadAssetArray<EffectConstant>();
			m_effectGUIDs = reader.ReadAssetArray<EngineGUID>();
			NumSideChainBuffers = reader.ReadUInt32();
			m_snapshots = reader.ReadAssetArray<SnapshotConstant>();
			m_snapshotGUIDs = reader.ReadAssetArray<EngineGUID>();
			//m_groupNameBuffer = stream.ReadArray<char>();
			reader.AlignStream(AlignType.Align4);
			
			//m_snapshotNameBuffer = stream.ReadArray<char>();
			reader.AlignStream(AlignType.Align4);
			
			//m_pluginEffectNameBuffer = stream.ReadArray<char>();
			reader.AlignStream(AlignType.Align4);
			
			m_exposedParameterNames = reader.ReadUInt32Array();
			m_exposedParameterIndices = reader.ReadUInt32Array();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			//node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("groups", Groups.ExportYAML(container));
			node.Add("groupGUIDs", GroupGUIDs.ExportYAML(container));
			node.Add("effects", Effects.ExportYAML(container));
			node.Add("effectGUIDs", EffectGUIDs.ExportYAML(container));
			node.Add("numSideChainBuffers", NumSideChainBuffers);
			node.Add("snapshots", Snapshots.ExportYAML(container));
			node.Add("snapshotGUIDs", SnapshotGUIDs.ExportYAML(container));
			//node.Add("groupNameBuffer", GroupNameBuffer.ExportYAML(container));
			//node.Add("snapshotNameBuffer", SnapshotNameBuffer.ExportYAML(container));
			//node.Add("pluginEffectNameBuffer", PluginEffectNameBuffer.ExportYAML(container));
			node.Add("exposedParameterNames", ExposedParameterNames.ExportYAML(true));
			node.Add("exposedParameterIndices", ExposedParameterIndices.ExportYAML(true));
			return node;
		}

		public IReadOnlyList<GroupConstant> Groups => m_groups;
		public IReadOnlyList<EngineGUID> GroupGUIDs => m_groupGUIDs;
		public IReadOnlyList<EffectConstant> Effects => m_effects;
		public IReadOnlyList<EngineGUID> EffectGUIDs => m_effectGUIDs;
		public uint NumSideChainBuffers { get; private set; }
		public IReadOnlyList<SnapshotConstant> Snapshots => m_snapshots;
		public IReadOnlyList<EngineGUID> SnapshotGUIDs => m_snapshotGUIDs;
		public IReadOnlyList<char> GroupNameBuffer => m_groupNameBuffer;
		public IReadOnlyList<char> SnapshotNameBuffer => m_snapshotNameBuffer;
		public IReadOnlyList<char> PluginEffectNameBuffer => m_pluginEffectNameBuffer;
		public IReadOnlyList<uint> ExposedParameterNames => m_exposedParameterNames;
		public IReadOnlyList<uint> ExposedParameterIndices => m_exposedParameterIndices;

		private GroupConstant[] m_groups;
		private EngineGUID[] m_groupGUIDs;
		private EffectConstant[] m_effects;
		private EngineGUID[] m_effectGUIDs;
		private SnapshotConstant[] m_snapshots;
		private EngineGUID[] m_snapshotGUIDs;
		private char[] m_groupNameBuffer;
		private char[] m_snapshotNameBuffer;
		private char[] m_pluginEffectNameBuffer;
		private uint[] m_exposedParameterNames;
		private uint[] m_exposedParameterIndices;
	}
}
