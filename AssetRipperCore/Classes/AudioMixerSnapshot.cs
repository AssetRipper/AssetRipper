using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
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

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.Add(AudioMixerName, AudioMixer.ExportYaml(container));
			node.Add(SnapshotIDName, SnapshotID.ExportYaml(container));
			return node;
		}

		public const string AudioMixerName = "m_AudioMixer";
		public const string SnapshotIDName = "m_SnapshotID";

		public PPtr<AudioMixer.AudioMixer> AudioMixer = new();
		public UnityGUID SnapshotID = new();
	}
}
