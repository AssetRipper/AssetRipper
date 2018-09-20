namespace uTinyRipper.Classes.Shaders
{
	public enum StencilOp
	{
		Keep		= 0,
		Zero		= 1,
		Replace		= 2,
		IncrSat		= 3,
		DecrSat		= 4,
		Invert		= 5,
		IncrWrap	= 6,
		DecrWrap	= 7,
		Count,
	}

	public static class StencilOpExtensions
	{
		public static bool IsKeep(this StencilOp _this)
		{
			return _this == StencilOp.Keep;
		}
	}
}
