namespace uTinyRipper.Classes.Shaders
{
	public enum BlendOp
	{
		Add					= 0,
		Sub					= 1,
		RevSub				= 2,
		Min					= 3,
		Max					= 4,
		LogicalClear		= 5,
		LogicalSet			= 6,
		LogicalCopy			= 7,
		LogicalCopyInverted	= 8,
		LogicalNoop			= 9,
		LogicalInvert		= 10,
		LogicalAnd			= 11,
		LogicalNand			= 12,
		LogicalOr			= 13,
		LogicalNor			= 14,
		LogicalXor			= 15,
		LogicalEquiv		= 16,
		LogicalAndReverse	= 17,
		LogicalAndInverted	= 18,
		LogicalOrReverse	= 19,
		LogicalOrInverted	= 20,
		Count,
	}

	public static class BlendOpExtensions
	{
		public static bool IsAdd(this BlendOp _this)
		{
			return _this == BlendOp.Add;
		}
	}
}
