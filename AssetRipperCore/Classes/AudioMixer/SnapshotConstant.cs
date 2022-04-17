using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;

namespace AssetRipper.Core.Classes.AudioMixer
{
#warning TODO: not implemented
	public sealed class SnapshotConstant : IAssetReadable, IYamlExportable
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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			//node.AddSerializedVersion(ToSerializedVersion(container.Version));
			node.Add(NameHashName, NameHash);
			node.Add(ValuesName, Values.ExportYaml());
			node.Add(TransitionTypesName, TransitionTypes.ExportYaml(true));
			node.Add(TransitionIndicesName, TransitionIndices.ExportYaml(true));
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
