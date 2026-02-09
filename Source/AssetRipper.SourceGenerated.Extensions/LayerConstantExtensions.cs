using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.LayerConstant;

namespace AssetRipper.SourceGenerated.Extensions;

public static class LayerConstantExtensions
{
	public static AnimatorLayerBlendingMode GetLayerBlendingMode(this ILayerConstant constant)
	{
		return (AnimatorLayerBlendingMode)constant.LayerBlendingMode;
	}
}
