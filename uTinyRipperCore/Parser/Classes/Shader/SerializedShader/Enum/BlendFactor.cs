namespace uTinyRipper.Classes.Shaders
{
	public enum BlendFactor
	{
		Zero				= 0,
		One					= 1,
		DstColor			= 2,
		SrcColor			= 3,
		OneMinusDstColor	= 4,
		SrcAlpha			= 5,
		OneMinusSrcColor	= 6,
		DstAlpha			= 7,
		OneMinusDstAlpha	= 8,
		SrcAlphaSaturate	= 9,
		OneMinusSrcAlpha	= 10,
		Count,
	}

	public static class BlendFactorExtensions
	{
		public static bool IsZero(this BlendFactor _this)
		{
			return _this == BlendFactor.Zero;
		}

		public static bool IsOne(this BlendFactor _this)
		{
			return _this == BlendFactor.One;
		}
	}
}
