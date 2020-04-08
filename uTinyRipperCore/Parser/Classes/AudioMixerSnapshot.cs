using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.Classes.Misc;

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

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}
			
			yield return context.FetchDependency(AudioMixer, AudioMixerName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(AudioMixerName, AudioMixer.ExportYAML(container));
			node.Add(SnapshotIDName, SnapshotID.ExportYAML(container));
			return node;
		}

		public const string AudioMixerName = "m_AudioMixer";
		public const string SnapshotIDName = "m_SnapshotID";

		public PPtr<AudioMixer> AudioMixer;
		public UnityGUID SnapshotID;
	}
}
