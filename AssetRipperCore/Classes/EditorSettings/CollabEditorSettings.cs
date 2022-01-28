using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.EditorSettings
{
	public sealed class CollabEditorSettings : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			InProgressEnabled = reader.ReadBoolean();
			reader.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(InProgressEnabledName, InProgressEnabled);
			return node;
		}

		public bool InProgressEnabled { get; set; } = true;

		public const string InProgressEnabledName = "inProgressEnabled";
	}
}
