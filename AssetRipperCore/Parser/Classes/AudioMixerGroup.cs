using AssetRipper.Converters.Project;
using AssetRipper.Parser.Asset;
using AssetRipper.Parser.Classes.Misc;
using AssetRipper.IO.Asset;
using AssetRipper.YAML;
using System;
using System.Collections.Generic;

namespace AssetRipper.Parser.Classes
{
#warning TODO: not implemented
	public sealed class AudioMixerGroup : NamedObject
	{
		public AudioMixerGroup(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			AudioMixer.Read(reader);
			GroupID.Read(reader);
			Children = reader.ReadAssetArray<PPtr<AudioMixerGroup>>();
		}

		public override IEnumerable<PPtr<Object.Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object.Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(AudioMixer, AudioMixerName);
			foreach (PPtr<Object.Object> asset in context.FetchDependencies(Children, ChildrenName))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public PPtr<AudioMixerGroup>[] Children { get; set; }

		public const string AudioMixerName = "m_AudioMixer";
		public const string ChildrenName = "m_Children";

		public PPtr<AudioMixer.AudioMixer> AudioMixer;
		public UnityGUID GroupID;
	}
}
