using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimatorControllers.Editor
{
	public sealed class AnimatorControllerParameter : IYAMLExportable
	{
		public AnimatorControllerParameter(string name, AnimatorControllerParameterType type, AnimatorController controller)
		{
			Name = name;
			Type = type;
			DefaultController = new PPtr<AnimatorController>(controller);
		}
		
		public AnimatorControllerParameter(string name, AnimatorControllerParameterType type, AnimatorController controller, bool @default) :
			this(name, type, controller)
		{
			DefaultBool = @default;
		}

		public AnimatorControllerParameter(string name, AnimatorControllerParameterType type, AnimatorController controller, int @default) :
			this(name, type, controller)
		{
			DefaultInt = @default;
		}

		public AnimatorControllerParameter(string name, AnimatorControllerParameterType type, AnimatorController controller, float @default) :
			this(name, type, controller)
		{
			DefaultFloat = @default;
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Name", Name);
			node.Add("m_Type", (int)Type);
			node.Add("m_DefaultFloat", DefaultFloat);
			node.Add("m_DefaultInt", DefaultInt);
			node.Add("m_DefaultBool", DefaultBool);
			node.Add("m_DefaultController", DefaultController.ExportYAML(container));
			return node;
		}

		public string Name { get; private set; }
		public AnimatorControllerParameterType Type { get; private set; }
		public float DefaultFloat { get; private set; }
		public int DefaultInt { get; private set; }
		public bool DefaultBool { get; private set; }

		public PPtr<AnimatorController> DefaultController;
	}
}
