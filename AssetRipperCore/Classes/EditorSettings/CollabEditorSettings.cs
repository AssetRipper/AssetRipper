using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.EditorSettings
{
	public sealed class CollabEditorSettings : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			InProgressEnabled = reader.ReadBoolean();
			reader.AlignStream();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(InProgressEnabledName, InProgressEnabled);
			return node;
		}

		public bool InProgressEnabled { get; set; } = true;

		public const string InProgressEnabledName = "inProgressEnabled";
	}
}
