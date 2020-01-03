namespace uTinyRipper.YAML
{
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

		/// <summary>
		/// Single line with hex data
		/// </summary>
		Raw,
	}

	public static class SequenceStyleExtensions
	{
		public static bool IsRaw(this SequenceStyle _this)
		{
			return _this == SequenceStyle.Raw;
		}

		public static bool IsAnyBlock(this SequenceStyle _this)
		{
			return _this == SequenceStyle.Block || _this == SequenceStyle.BlockCurve;
		}

		/// <summary>
		/// Get scalar style corresponding to current sequence style
		/// </summary>
		/// <param name="_this">Sequence style</param>
		/// <returns>Corresponding scalar style</returns>
		public static ScalarStyle ToScalarStyle(this SequenceStyle _this)
		{
			return _this == SequenceStyle.Raw ? ScalarStyle.Hex : ScalarStyle.Plain;
		}
	}
}
