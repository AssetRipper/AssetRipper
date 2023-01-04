using AssetRipper.Assets.Generics;
using AssetRipper.SourceGenerated.Extensions.Enums.AnimationClip.GenericBinding;
using AssetRipper.SourceGenerated.Subclasses.AnimationClipBindingConstant;
using AssetRipper.SourceGenerated.Subclasses.GenericBinding;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class AnimationClipBindingConstantExtensions
	{
		public static IGenericBinding FindBinding(this IAnimationClipBindingConstant constant, int index)
		{
			int curves = 0;
			AccessListBase<IGenericBinding> bindings = constant.GenericBindings;

			for (int i = 0; i < bindings.Count; i++)
			{
				IGenericBinding gb = bindings[i];
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
