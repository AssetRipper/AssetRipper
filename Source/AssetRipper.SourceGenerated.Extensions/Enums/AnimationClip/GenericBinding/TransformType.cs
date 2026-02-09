namespace AssetRipper.SourceGenerated.Extensions.Enums.AnimationClip.GenericBinding;

public enum TransformType
{
	Translation = 1,
	Rotation = 2,
	Scaling = 3,
	EulerRotation = 4,
}

public static class TransformTypeExtensions
{
	public static bool IsValid(this TransformType _this)
	{
		return _this >= TransformType.Translation && _this <= TransformType.EulerRotation;
	}

	public static int GetDimension(this TransformType _this)
	{
		switch (_this)
		{
			case TransformType.Translation:
			case TransformType.Scaling:
			case TransformType.EulerRotation:
				return 3;

			case TransformType.Rotation:
				return 4;

			default:
				throw new NotImplementedException($"Binding type {_this} is not implemented");
		}
	}
}
