using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes
{
#warning TODO: not implemented
	public sealed class AudioMixerGroup : NamedObject
	{
		public AudioMixerGroup(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			AudioMixer.Read(reader);
			GroupID.Read(reader);
			Children = reader.ReadAssetArray<PPtr<AudioMixerGroup>>();
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(AudioMixer, AudioMixerName);
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(Children, ChildrenName))
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

		public PPtr<AudioMixer.AudioMixer> AudioMixer = new();
		public UnityGUID GroupID = new();
	}
}
