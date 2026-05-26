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

	public static int GetDimension(this TransformType _this) => _this switch
	{
		TransformType.Translation or TransformType.Scaling or TransformType.EulerRotation => 3,
		TransformType.Rotation => 4,
		_ => throw new ArgumentOutOfRangeException(nameof(_this), _this, $"Invalid {nameof(TransformType)} value"),
	};
}
