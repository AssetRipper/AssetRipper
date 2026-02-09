using AssetRipper.SourceGenerated.NativeEnums.Animation;
using AssetRipper.SourceGenerated.Subclasses.ValueConstant;

namespace AssetRipper.SourceGenerated.Extensions;

public static class ValueConstantExtensions
{
	public static AnimatorControllerParameterType GetTypeValue(this IValueConstant valueConstant)
	{
		return (AnimatorControllerParameterType)valueConstant.Type;
	}
}
