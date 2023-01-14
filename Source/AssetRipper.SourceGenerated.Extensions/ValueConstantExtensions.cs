using AssetRipper.SourceGenerated.Subclasses.ValueConstant;
using AnimatorControllerParameterType = AssetRipper.SourceGenerated.Enums.AnimatorControllerParameterType_1;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class ValueConstantExtensions
	{
		public static AnimatorControllerParameterType GetTypeValue(this IValueConstant valueConstant)
		{
			return (AnimatorControllerParameterType)valueConstant.Type;
		}
	}
}
