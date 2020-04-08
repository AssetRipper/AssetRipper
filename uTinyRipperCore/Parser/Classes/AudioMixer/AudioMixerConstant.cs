using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper.Classes.Misc;
using uTinyRipper;

namespace uTinyRipper.Classes.AudioMixers
{
#warning TODO: not implemented
	public struct AudioMixerConstant : IAssetReadable, IYAMLExportable
	{
		/*public static int ToSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}*/

		public void Read(AssetReader reader)
		{
			Groups = reader.ReadAssetArray<GroupConstant>();
			GroupGUIDs = reader.ReadAssetArray<UnityGUID>();
			Effects = reader.ReadAssetArray<EffectConstant>();
			EffectGUIDs = reader.ReadAssetArray<UnityGUID>();
			NumSideChainBuffers = reader.ReadUInt32();
			Snapshots = reader.ReadAssetArray<SnapshotConstant>();
			SnapshotGUIDs = reader.ReadAssetArray<UnityGUID>();
			//m_groupNameBuffer = stream.ReadArray<char>();
			reader.AlignStream();
			
			//m_snapshotNameBuffer = stream.ReadArray<char>();
			reader.AlignStream();
			
			//m_pluginEffectNameBuffer = stream.ReadArray<char>();
			reader.AlignStream();
			
			ExposedParameterNames = reader.ReadUInt32Array();
			ExposedParameterIndices = reader.ReadUInt32Array();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			//node.AddSerializedVersion(ToSerializedVersion(container.Version));
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

		public GroupConstant[] Groups { get; set; }
		public UnityGUID[] GroupGUIDs { get; set; }
		public EffectConstant[] Effects { get; set; }
		public UnityGUID[] EffectGUIDs { get; set; }
		public uint NumSideChainBuffers { get; set; }
		public SnapshotConstant[] Snapshots { get; set; }
		public UnityGUID[] SnapshotGUIDs { get; set; }
		public char[] GroupNameBuffer { get; set; }
		public char[] SnapshotNameBuffer { get; set; }
		public char[] PluginEffectNameBuffer { get; set; }
		public uint[] ExposedParameterNames { get; set; }
		public uint[] ExposedParameterIndices { get; set; }

		public const string GroupsName = "groups";
		public const string GroupGUIDsName = "groupGUIDs";
		public const string EffectsName = "effects";
		public const string EffectGUIDsName = "effectGUIDs";
		public const string NumSideChainBuffersName = "numSideChainBuffers";
		public const string SnapshotsName = "snapshots";
		public const string SnapshotGUIDsName = "snapshotGUIDs";
		public const string ExposedParameterNamesName = "exposedParameterNames";
		public const string ExposedParameterIndicesName = "exposedParameterIndices";
	}
}
