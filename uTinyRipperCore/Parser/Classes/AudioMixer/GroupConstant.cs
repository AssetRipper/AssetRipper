using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AudioMixers
{
#warning TODO: not implemented
	public struct GroupConstant : IAssetReadable, IYAMLExportable
	{
		/*public static int ToSerializedVersion(Version version)
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
			reader.AlignStream();
			
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			//node.AddSerializedVersion(ToSerializedVersion(container.Version));
			node.Add(ParentConstantIndexName, ParentConstantIndex);
			node.Add(VolumeIndexName, VolumeIndex);
			node.Add(PitchIndexName, PitchIndex);
			node.Add(MuteName, Mute);
			node.Add(SoloName, Solo);
			node.Add(BypassEffectsName, BypassEffects);
			return node;
		}

		public int ParentConstantIndex { get; set; }
		public uint VolumeIndex { get; set; }
		public uint PitchIndex { get; set; }
		public bool Mute { get; set; }
		public bool Solo { get; set; }
		public bool BypassEffects { get; set; }

		public const string ParentConstantIndexName = "parentConstantIndex";
		public const string VolumeIndexName = "volumeIndex";
		public const string PitchIndexName = "pitchIndex";
		public const string MuteName = "mute";
		public const string SoloName = "solo";
		public const string BypassEffectsName = "bypassEffects";
	}
}
