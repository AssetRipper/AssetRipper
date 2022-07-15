using AssetRipper.Core.Classes.AnimatorController.Editor.AnimatorControllerParameter;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Subclasses.AnimatorControllerParameter;
using AssetRipper.SourceGenerated.Subclasses.ValueConstant;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class AnimatorControllerParameterExtensions
	{
		public static void Initialize(this IAnimatorControllerParameter parameter, IAnimatorController controller, int paramIndex)
		{
			IValueConstant value = controller.Controller_C91.Values.Data.ValueArray[paramIndex];
			parameter.Name.CopyValues(controller.TOS_C91[value.ID]);
			AnimatorControllerParameterType type = value.GetTypeValue();
			switch (type)
			{
				case AnimatorControllerParameterType.Trigger:
					parameter.DefaultBool = controller.Controller_C91.DefaultValues.Data.BoolValues[value.Index];
					break;

				case AnimatorControllerParameterType.Bool:
					parameter.DefaultBool = controller.Controller_C91.DefaultValues.Data.BoolValues[value.Index];
					break;

				case AnimatorControllerParameterType.Int:
					parameter.DefaultInt = controller.Controller_C91.DefaultValues.Data.IntValues[value.Index];
					break;

				case AnimatorControllerParameterType.Float:
					parameter.DefaultFloat = controller.Controller_C91.DefaultValues.Data.FloatValues[value.Index];
					break;

				default:
					throw new NotSupportedException($"Parameter type '{type}' isn't supported");
			}
			parameter.Type = (int)type;
			if (parameter.Has_Controller())
			{
				parameter.Controller.CopyValues(controller.SerializedFile.CreatePPtr(controller));
			}
		}

		public static AnimatorControllerParameterType GetTypeValue(this IAnimatorControllerParameter parameter)
		{
			return (AnimatorControllerParameterType)parameter.Type;
		}
	}
}
