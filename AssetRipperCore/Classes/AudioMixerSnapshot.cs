using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes
{
	public sealed class AudioMixerSnapshot : NamedObject
	{
		public AudioMixerSnapshot(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			AudioMixer.Read(reader);
			SnapshotID.Read(reader);
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
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

		public PPtr<AudioMixer.AudioMixer> AudioMixer = new();
		public UnityGUID SnapshotID = new();
	}
}
