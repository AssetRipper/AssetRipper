using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AudioMixers
{
#warning TODO: not implemented
	public struct GroupConstant : IAssetReadable, IYAMLExportable
	{
		/*private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}*/

		public void Read(AssetReader reader)
		{
			ParentConstantIndex = reader.ReadInt32();
			VolumeIndex = reader.ReadUInt32();
			PitchIndex = reader.ReadUInt32();
			Mute = reader.ReadBoolean();
			Solo = reader.ReadBoolean();
			BypassEffects = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
			
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			//node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("parentConstantIndex", ParentConstantIndex);
			node.Add("volumeIndex", VolumeIndex);
			node.Add("pitchIndex", PitchIndex);
			node.Add("mute", Mute);
			node.Add("solo", Solo);
			node.Add("bypassEffects", BypassEffects);
			return node;
		}

		public int ParentConstantIndex { get; private set; }
		public uint VolumeIndex { get; private set; }
		public uint PitchIndex { get; private set; }
		public bool Mute { get; private set; }
		public bool Solo { get; private set; }
		public bool BypassEffects { get; private set; }
	}
}
