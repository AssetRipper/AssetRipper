using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
#warning TODO: not implemented
	public sealed class AudioMixerSnapshot : NamedObject
	{
		public AudioMixerSnapshot(AssetInfo assetInfo) : base(assetInfo)
		{
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			AudioMixer.Read(stream);
			SnapshotID.Read(stream);
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
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
		public UtinyGUID SnapshotID;
	}
}
