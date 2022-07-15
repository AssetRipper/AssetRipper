using AssetRipper.Core.Classes.AnimationClip.GenericBinding;
using AssetRipper.SourceGenerated.Subclasses.AnimationClipBindingConstant;
using AssetRipper.SourceGenerated.Subclasses.GenericBinding;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class AnimationClipBindingConstantExtensions
	{
		public static IGenericBinding FindBinding(this IAnimationClipBindingConstant constant, int index)
		{
			int curves = 0;
			for (int i = 0; i < constant.GenericBindings.Count; i++)
			{
				IGenericBinding gb = constant.GenericBindings[i];
				if (gb.GetClassID() == ClassIDType.Transform)
				{
					curves += gb.TransformType().GetDimension();
				}
				else
				{
					curves += 1;
				}

				if (curves > index)
				{
					return gb;
				}
			}
			throw new ArgumentException($"Binding with index {index} hasn't been found", nameof(index));
		}
	}
}
