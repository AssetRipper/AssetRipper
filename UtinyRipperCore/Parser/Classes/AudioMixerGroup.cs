using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
#warning TODO: not implemented
	public sealed class AudioMixerGroup : NamedObject
	{
		public AudioMixerGroup(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			AudioMixer.Read(stream);
			GroupID.Read(stream);
			m_children = stream.ReadArray<PPtr<AudioMixerGroup>>();
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
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
		public UtinyGUID GroupID;

		private PPtr<AudioMixerGroup>[] m_children;
	}
}
