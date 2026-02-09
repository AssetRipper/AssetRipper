using AssetRipper.Assets.Generics;
using AssetRipper.SourceGenerated.Subclasses.AnimationClipBindingConstant;
using AssetRipper.SourceGenerated.Subclasses.Clip;
using AssetRipper.SourceGenerated.Subclasses.GenericBinding;
using AssetRipper.SourceGenerated.Subclasses.OffsetPtr_ValueArrayConstant;

namespace AssetRipper.SourceGenerated.Extensions;

public static class ClipExtensions
{
	public static bool IsSet(this IClip clip)
	{
		return clip.StreamedClip.IsSet()
			|| clip.DenseClip.IsSet()
			|| clip.Has_ConstantClip() && clip.ConstantClip.IsSet();
	}

	public static void ConvertValueArrayToGenericBinding(this IClip clip, IAnimationClipBindingConstant bindings)
	{
		if (clip.Has_Binding())
		{
			AccessListBase<IGenericBinding> genericBindings = bindings.GenericBindings;
			IOffsetPtr_ValueArrayConstant values = clip.Binding;
			for (int i = 0; i < values.Data.ValueArray.Count;)
			{
				uint curveID = values.Data.ValueArray[i].ID;
				uint curveTypeID = values.Data.ValueArray[i].TypeID;
				IGenericBinding binding = genericBindings.AddNew();

				if (curveTypeID == 4174552735) //CRC(PositionX))
				{
					binding.Path = curveID;
					binding.Attribute = 1; //kBindTransformPosition
					binding.SetClassID(ClassIDType.Transform);
					i += 3;
				}
				else if (curveTypeID == 2211994246) //CRC(QuaternionX))
				{
					binding.Path = curveID;
					binding.Attribute = 2; //kBindTransformRotation
					binding.SetClassID(ClassIDType.Transform);
					i += 4;
				}
				else if (curveTypeID == 1512518241) //CRC(ScaleX))
				{
					binding.Path = curveID;
					binding.Attribute = 3; //kBindTransformScale
					binding.SetClassID(ClassIDType.Transform);
					i += 3;
				}
				else
				{
					binding.Path = 0;
					binding.Attribute = curveID;
					binding.SetClassID(ClassIDType.Animator);
					i++;
				}
			}
		}
	}
}
