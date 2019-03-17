using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.EditorSettingss
{
	public struct CollabEditorSettings : IAssetReadable, IYAMLExportable
	{
		public CollabEditorSettings(bool _)
		{
			InProgressEnabled = true;
		}

		public void Read(AssetReader reader)
		{
			InProgressEnabled = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("inProgressEnabled", InProgressEnabled);
			return node;
		}

		public bool InProgressEnabled { get; private set; }
	}
}
