using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.AudioMixers;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
#warning TODO: not implemented
	public sealed class AudioMixer : NamedObject
	{
		public AudioMixer(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/*private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}*/

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			OutputGroup.Read(stream);
			MasterGroup.Read(stream);
			m_snapshots = stream.ReadArray<PPtr<AudioMixerSnapshot>>();
			StartSnapshot.Read(stream);
			SuspendThreshold = stream.ReadSingle();
			EnableSuspend = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
			
			UpdateMode = stream.ReadInt32();
			stream.AlignStream(AlignType.Align4);
			
			MixerConstant.Read(stream);
			stream.AlignStream(AlignType.Align4);
			
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			//node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_OutputGroup", OutputGroup.ExportYAML(container));
			node.Add("m_MasterGroup", MasterGroup.ExportYAML(container));
			node.Add("m_Snapshots", Snapshots.ExportYAML(container));
			node.Add("m_StartSnapshot", StartSnapshot.ExportYAML(container));
			node.Add("m_SuspendThreshold", SuspendThreshold);
			node.Add("m_EnableSuspend", EnableSuspend);
			node.Add("m_UpdateMode", UpdateMode);
			node.Add("m_MixerConstant", MixerConstant.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<PPtr<AudioMixerSnapshot>> Snapshots => m_snapshots;
		public float SuspendThreshold { get; private set; }
		public bool EnableSuspend { get; private set; }
		public int UpdateMode { get; private set; }

		public PPtr<AudioMixerGroup> OutputGroup;
		public PPtr<AudioMixerGroup> MasterGroup;
		public PPtr<AudioMixerSnapshot> StartSnapshot;
		public AudioMixerConstant MixerConstant;

		private PPtr<AudioMixerSnapshot>[] m_snapshots;
	}
}
