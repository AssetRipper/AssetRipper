using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
#warning TODO: not implemented
	public sealed class AudioMixerGroup : NamedObject
	{
		public AudioMixerGroup(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			AudioMixer.Read(reader);
			GroupID.Read(reader);
			m_children = reader.ReadAssetArray<PPtr<AudioMixerGroup>>();
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			foreach (PPtr<AudioMixerGroup> group in Children)
			{
				yield return group.FetchDependency(file, isLog, ToLogString, "Children");
			}
			yield return AudioMixer.FetchDependency(file, isLog, ToLogString, "AudioMixer");
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public IReadOnlyList<PPtr<AudioMixerGroup>> Children => m_children;

		public PPtr<AudioMixer> AudioMixer;
		public EngineGUID GroupID;

		private PPtr<AudioMixerGroup>[] m_children;
	}
}
