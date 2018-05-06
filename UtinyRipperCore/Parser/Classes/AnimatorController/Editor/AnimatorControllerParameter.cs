using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimatorControllers.Editor
{
	public sealed class AnimatorControllerParameter : IYAMLExportable
	{
		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Name", Name);
			node.Add("m_Type", Type);
			node.Add("m_DefaultFloat", DefaultFloat);
			node.Add("m_DefaultInt", DefaultInt);
			node.Add("m_DefaultBool", DefaultBool);
			node.Add("m_DefaultController", DefaultController.ExportYAML(container));
			return node;
		}

		public string Name { get; private set; }
		public int Type { get; private set; }
		public float DefaultFloat { get; private set; }
		public int DefaultInt { get; private set; }
		public bool DefaultBool { get; private set; }

		public PPtr<AnimatorController> DefaultController;
	}
}
