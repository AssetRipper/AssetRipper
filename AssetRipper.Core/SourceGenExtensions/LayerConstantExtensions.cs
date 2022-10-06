using AssetRipper.SourceGenerated.Subclasses.LayerConstant;
using AnimatorLayerBlendingMode = AssetRipper.SourceGenerated.Enums.AnimatorLayerBlendingMode_0;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class LayerConstantExtensions
	{
		public static AnimatorLayerBlendingMode GetLayerBlendingMode(this ILayerConstant constant)
		{
			return (AnimatorLayerBlendingMode)constant.LayerBlendingMode;
		}
	}
}
