using AssetRipper.Core.Classes.AnimatorController.Editor.AnimatorControllerParameter;
using AssetRipper.SourceGenerated.Subclasses.ValueConstant;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class ValueConstantExtensions
	{
		public static AnimatorControllerParameterType GetTypeValue(this IValueConstant valueConstant)
		{
			return (AnimatorControllerParameterType)valueConstant.Type;
		}
	}
}
