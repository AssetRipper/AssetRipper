using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.AudioMixer
{
#warning TODO: not implemented
	public sealed class AudioMixer : NamedObject
	{
		public AudioMixer(AssetInfo assetInfo) : base(assetInfo) { }

		/*public static int ToSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}*/

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			OutputGroup.Read(reader);
			MasterGroup.Read(reader);
			Snapshots = reader.ReadAssetArray<PPtr<AudioMixerSnapshot>>();
			StartSnapshot.Read(reader);
			SuspendThreshold = reader.ReadSingle();
			EnableSuspend = reader.ReadBoolean();
			reader.AlignStream();

			UpdateMode = reader.ReadInt32();
			reader.AlignStream();

			MixerConstant.Read(reader);
			reader.AlignStream();

		}

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			//node.AddSerializedVersion(ToSerializedVersion(container.Version));
			node.Add(OutputGroupName, OutputGroup.ExportYaml(container));
			node.Add(MasterGroupName, MasterGroup.ExportYaml(container));
			node.Add(SnapshotsName, Snapshots.ExportYaml(container));
			node.Add(StartSnapshotName, StartSnapshot.ExportYaml(container));
			node.Add(SuspendThresholdName, SuspendThreshold);
			node.Add(EnableSuspendName, EnableSuspend);
			node.Add(UpdateModeName, UpdateMode);
			node.Add(MixerConstantName, MixerConstant.ExportYaml(container));
			return node;
		}

		public PPtr<AudioMixerSnapshot>[] Snapshots { get; set; }
		public float SuspendThreshold { get; set; }
		public bool EnableSuspend { get; set; }
		public int UpdateMode { get; set; }

		public const string OutputGroupName = "m_OutputGroup";
		public const string MasterGroupName = "m_MasterGroup";
		public const string SnapshotsName = "m_Snapshots";
		public const string StartSnapshotName = "m_StartSnapshot";
		public const string SuspendThresholdName = "m_SuspendThreshold";
		public const string EnableSuspendName = "m_EnableSuspend";
		public const string UpdateModeName = "m_UpdateMode";
		public const string MixerConstantName = "m_MixerConstant";

		public PPtr<AudioMixerGroup> OutputGroup = new();
		public PPtr<AudioMixerGroup> MasterGroup = new();
		public PPtr<AudioMixerSnapshot> StartSnapshot = new();
		public AudioMixerConstant MixerConstant = new();
	}
}
