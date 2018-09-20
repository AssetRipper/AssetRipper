namespace uTinyRipper.Classes.Shaders
{
	public enum FogMode
	{
		Off		= 0,
		Linear	= 1,
		Exp		= 2,
		Exp2	= 3,
		Count,

		Unknown	= -1,
	}

	public static class FogModeExtensions
	{
		public static bool IsUnknown(this FogMode _this)
		{
			return _this == FogMode.Unknown;
		}
	}
}
