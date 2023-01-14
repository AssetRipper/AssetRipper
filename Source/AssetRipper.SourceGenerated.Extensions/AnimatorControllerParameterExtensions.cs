using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.AnimatorControllerParameter;
using AssetRipper.SourceGenerated.Subclasses.ValueConstant;
using AnimatorControllerParameterType = AssetRipper.SourceGenerated.Enums.AnimatorControllerParameterType_1;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class AnimatorControllerParameterExtensions
	{
		/// <summary>
		/// Initialize a newly created parameter
		/// </summary>
		/// <param name="parameter">Must be a child of <paramref name="controller"/></param>
		/// <param name="controller"></param>
		/// <param name="paramIndex"></param>
		/// <exception cref="NotSupportedException"></exception>
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
				parameter.Controller.CopyValues(controller.Collection.CreatePPtr(controller));
			}
		}

		public static AnimatorControllerParameterType GetTypeValue(this IAnimatorControllerParameter parameter)
		{
			return (AnimatorControllerParameterType)parameter.Type;
		}
	}
}
