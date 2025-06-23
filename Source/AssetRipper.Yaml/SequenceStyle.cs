namespace AssetRipper.Yaml;

/// <summary>
/// Specifies the style of a sequence.
/// </summary>
public enum SequenceStyle
{
	/// <summary>
	/// The block sequence style
	/// </summary>
	Block,

	/// <summary>
	/// The block sequence style but with curly braces
	/// </summary>
	BlockCurve,

	/// <summary>
	/// The flow sequence style
	/// </summary>
	Flow,
}

public static class SequenceStyleExtensions
{
	public static bool IsAnyBlock(this SequenceStyle _this)
	{
		return _this is SequenceStyle.Block or SequenceStyle.BlockCurve;
	}
}
