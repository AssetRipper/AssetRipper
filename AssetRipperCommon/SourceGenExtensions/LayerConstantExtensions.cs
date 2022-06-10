using AssetRipper.Core.Classes.AnimatorController.Editor.AnimatorControllerLayer;
using AssetRipper.SourceGenerated.Subclasses.LayerConstant;

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
