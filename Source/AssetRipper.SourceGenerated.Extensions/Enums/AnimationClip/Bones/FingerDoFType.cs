namespace AssetRipper.SourceGenerated.Extensions.Enums.AnimationClip.Bones;

public enum FingerDoFType
{
	_1Stretched = 0,
	Spread = 1,
	_2Stretched = 2,
	_3Stretched = 3,

	Last,
}

public static class FingerDoFTypeExtensions
{
	public static string ToAttributeString(this FingerDoFType _this)
	{
		return _this switch
		{
			FingerDoFType._1Stretched => "1 Stretched",
			FingerDoFType.Spread => "Spread",
			FingerDoFType._2Stretched => "2 Stretched",
			FingerDoFType._3Stretched => "3 Stretched",
			_ => throw new ArgumentException(_this.ToString()),
		};
	}
}
