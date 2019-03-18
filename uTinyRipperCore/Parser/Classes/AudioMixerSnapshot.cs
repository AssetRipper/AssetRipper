using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
#warning TODO: not implemented
	public sealed class AudioMixerSnapshot : NamedObject
	{
		public AudioMixerSnapshot(AssetInfo assetInfo) : base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			AudioMixer.Read(reader);
			SnapshotID.Read(reader);
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
			
			yield return AudioMixer.FetchDependency(file, isLog, ToLogString, "m_AudioMixer");
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_AudioMixer", AudioMixer.ExportYAML(container));
			node.Add("m_SnapshotID", SnapshotID.ExportYAML(container));
			return node;
		}

		public PPtr<AudioMixer> AudioMixer;
		public EngineGUID SnapshotID;
	}
}
