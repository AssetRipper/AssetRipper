using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AudioMixers
{
#warning TODO: not implemented
	public struct SnapshotConstant : IAssetReadable, IYAMLExportable
	{
		/*public static int ToSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}*/

		public void Read(AssetReader reader)
		{
			NameHash = reader.ReadUInt32();
			Values = reader.ReadSingleArray();
			TransitionTypes = reader.ReadUInt32Array();
			TransitionIndices = reader.ReadUInt32Array();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			//node.AddSerializedVersion(ToSerializedVersion(container.Version));
			node.Add(NameHashName, NameHash);
			node.Add(ValuesName, Values.ExportYAML());
			node.Add(TransitionTypesName, TransitionTypes.ExportYAML(true));
			node.Add(TransitionIndicesName, TransitionIndices.ExportYAML(true));
			return node;
		}

		public uint NameHash { get; set; }
		public float[] Values { get; set; }
		public uint[] TransitionTypes { get; set; }
		public uint[] TransitionIndices { get; set; }

		public const string NameHashName = "nameHash";
		public const string ValuesName = "values";
		public const string TransitionTypesName = "transitionTypes";
		public const string TransitionIndicesName = "transitionIndices";
	}
}
