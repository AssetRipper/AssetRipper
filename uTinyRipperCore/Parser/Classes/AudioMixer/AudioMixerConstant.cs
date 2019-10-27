using System.Collections.Generic;
using uTinyRipper.Project;
using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper.Classes.Misc;

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
			m_groupGUIDs = reader.ReadAssetArray<GUID>();
			m_effects = reader.ReadAssetArray<EffectConstant>();
			m_effectGUIDs = reader.ReadAssetArray<GUID>();
			NumSideChainBuffers = reader.ReadUInt32();
			m_snapshots = reader.ReadAssetArray<SnapshotConstant>();
			m_snapshotGUIDs = reader.ReadAssetArray<GUID>();
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
			node.Add(GroupsName, Groups.ExportYAML(container));
			node.Add(GroupGUIDsName, GroupGUIDs.ExportYAML(container));
			node.Add(EffectsName, Effects.ExportYAML(container));
			node.Add(EffectGUIDsName, EffectGUIDs.ExportYAML(container));
			node.Add(NumSideChainBuffersName, NumSideChainBuffers);
			node.Add(SnapshotsName, Snapshots.ExportYAML(container));
			node.Add(SnapshotGUIDsName, SnapshotGUIDs.ExportYAML(container));
			//node.Add("groupNameBuffer", GroupNameBuffer.ExportYAML(container));
			//node.Add("snapshotNameBuffer", SnapshotNameBuffer.ExportYAML(container));
			//node.Add("pluginEffectNameBuffer", PluginEffectNameBuffer.ExportYAML(container));
			node.Add(ExposedParameterNamesName, ExposedParameterNames.ExportYAML(true));
			node.Add(ExposedParameterIndicesName, ExposedParameterIndices.ExportYAML(true));
			return node;
		}

		public IReadOnlyList<GroupConstant> Groups => m_groups;
		public IReadOnlyList<GUID> GroupGUIDs => m_groupGUIDs;
		public IReadOnlyList<EffectConstant> Effects => m_effects;
		public IReadOnlyList<GUID> EffectGUIDs => m_effectGUIDs;
		public uint NumSideChainBuffers { get; private set; }
		public IReadOnlyList<SnapshotConstant> Snapshots => m_snapshots;
		public IReadOnlyList<GUID> SnapshotGUIDs => m_snapshotGUIDs;
		public IReadOnlyList<char> GroupNameBuffer => m_groupNameBuffer;
		public IReadOnlyList<char> SnapshotNameBuffer => m_snapshotNameBuffer;
		public IReadOnlyList<char> PluginEffectNameBuffer => m_pluginEffectNameBuffer;
		public IReadOnlyList<uint> ExposedParameterNames => m_exposedParameterNames;
		public IReadOnlyList<uint> ExposedParameterIndices => m_exposedParameterIndices;

		public const string GroupsName = "groups";
		public const string GroupGUIDsName = "groupGUIDs";
		public const string EffectsName = "effects";
		public const string EffectGUIDsName = "effectGUIDs";
		public const string NumSideChainBuffersName = "numSideChainBuffers";
		public const string SnapshotsName = "snapshots";
		public const string SnapshotGUIDsName = "snapshotGUIDs";
		public const string ExposedParameterNamesName = "exposedParameterNames";
		public const string ExposedParameterIndicesName = "exposedParameterIndices";

		private GroupConstant[] m_groups;
		private GUID[] m_groupGUIDs;
		private EffectConstant[] m_effects;
		private GUID[] m_effectGUIDs;
		private SnapshotConstant[] m_snapshots;
		private GUID[] m_snapshotGUIDs;
		private char[] m_groupNameBuffer;
		private char[] m_snapshotNameBuffer;
		private char[] m_pluginEffectNameBuffer;
		private uint[] m_exposedParameterNames;
		private uint[] m_exposedParameterIndices;
	}
}
