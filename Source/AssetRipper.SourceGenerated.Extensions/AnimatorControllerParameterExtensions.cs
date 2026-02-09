using AssetRipper.SourceGenerated.NativeEnums.Animation;
using AssetRipper.SourceGenerated.Subclasses.AnimatorControllerParameter;

namespace AssetRipper.SourceGenerated.Extensions;

public static class AnimatorControllerParameterExtensions
{
	public static AnimatorControllerParameterType GetTypeValue(this IAnimatorControllerParameter parameter)
	{
		return (AnimatorControllerParameterType)parameter.Type;
	}
}
