using System;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimatorControllers.Editor
{
	public sealed class AnimatorControllerParameter : IYAMLExportable
	{
		public AnimatorControllerParameter(AnimatorController controller, int paramIndex)
		{
			ValueConstant value = controller.Controller.Values.Instance.ValueArray[paramIndex];
			Name = controller.TOS[value.ID];
			Type = value.GetTypeValue(controller.File.Version);
			switch (Type)
			{
				case AnimatorControllerParameterType.Trigger:
					DefaultBool = controller.Controller.DefaultValues.Instance.BoolValues[value.Index];
					break;

				case AnimatorControllerParameterType.Bool:
					DefaultBool = controller.Controller.DefaultValues.Instance.BoolValues[value.Index];
					break;

				case AnimatorControllerParameterType.Int:
					DefaultInt = controller.Controller.DefaultValues.Instance.IntValues[value.Index];
					break;

				case AnimatorControllerParameterType.Float:
					DefaultFloat = controller.Controller.DefaultValues.Instance.FloatValues[value.Index];
					break;

				default:
					throw new NotSupportedException($"Parameter type '{Type}' isn't supported");
			}
			DefaultController = controller.File.CreatePPtr(controller);
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
