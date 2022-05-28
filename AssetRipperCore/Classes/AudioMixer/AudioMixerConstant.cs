using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;

namespace AssetRipper.Core.Classes.AudioMixer
{
#warning TODO: not implemented
	public sealed class AudioMixerConstant : IAssetReadable, IYamlExportable
	{
		/*public static int ToSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}*/

		/// <summary>
		/// 2021.2 and greater
		/// </summary>
		public static bool HasGroupConnections(UnityVersion version) => version.IsGreaterEqual(2021, 2);

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

			if (HasGroupConnections(reader.Version))
			{
				reader.ReadAssetArray<GroupConnection>();
			}
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			//node.AddSerializedVersion(ToSerializedVersion(container.Version));
			node.Add(GroupsName, Groups.ExportYaml(container));
			node.Add(GroupGUIDsName, GroupGUIDs.ExportYaml(container));
			node.Add(EffectsName, Effects.ExportYaml(container));
			node.Add(EffectGUIDsName, EffectGUIDs.ExportYaml(container));
			node.Add(NumSideChainBuffersName, NumSideChainBuffers);
			node.Add(SnapshotsName, Snapshots.ExportYaml(container));
			node.Add(SnapshotGUIDsName, SnapshotGUIDs.ExportYaml(container));
			//node.Add("groupNameBuffer", GroupNameBuffer.ExportYaml(container));
			//node.Add("snapshotNameBuffer", SnapshotNameBuffer.ExportYaml(container));
			//node.Add("pluginEffectNameBuffer", PluginEffectNameBuffer.ExportYaml(container));
			node.Add(ExposedParameterNamesName, ExposedParameterNames.ExportYaml(true));
			node.Add(ExposedParameterIndicesName, ExposedParameterIndices.ExportYaml(true));
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
