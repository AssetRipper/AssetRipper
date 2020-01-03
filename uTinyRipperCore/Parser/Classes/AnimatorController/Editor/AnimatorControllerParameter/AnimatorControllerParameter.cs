using System;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimatorControllers
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
			node.Add(NameName, Name);
			node.Add(TypeName, (int)Type);
			node.Add(DefaultFloatName, DefaultFloat);
			node.Add(DefaultIntName, DefaultInt);
			node.Add(DefaultBoolName, DefaultBool);
			node.Add(DefaultControllerName, DefaultController.ExportYAML(container));
			return node;
		}

		public string Name { get; set; }
		public AnimatorControllerParameterType Type { get; set; }
		public float DefaultFloat { get; set; }
		public int DefaultInt { get; set; }
		public bool DefaultBool { get; set; }

		public const string NameName = "m_Name";
		public const string TypeName = "m_Type";
		public const string DefaultFloatName = "m_DefaultFloat";
		public const string DefaultIntName = "m_DefaultInt";
		public const string DefaultBoolName = "m_DefaultBool";
		public const string DefaultControllerName = "m_DefaultController";

		public PPtr<AnimatorController> DefaultController;
	}
}
