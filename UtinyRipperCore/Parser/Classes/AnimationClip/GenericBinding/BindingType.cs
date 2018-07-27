using System;

namespace UtinyRipper.Classes.AnimationClips
{
	public enum BindingType
	{
		Translation		= 1,
		Rotation		= 2,
		Scaling			= 3,
		EulerRotation	= 4,
		Floats			= 5,
		Unknown1		= 7,
	}

	public static class BindingTypeExtensions
	{
		public static int GetSize(this BindingType _this)
		{
			switch(_this)
			{
				case BindingType.Translation:
				case BindingType.Scaling:
				case BindingType.EulerRotation:
					return 3;

				case BindingType.Rotation:
					return 4;

				case BindingType.Floats:
					return 1;

#warning TODO: AnimationClip
				default:
					return 1;
					//throw new NotImplementedException($"Binding type {_this} is not implemented");
			}
		}
	}
}
