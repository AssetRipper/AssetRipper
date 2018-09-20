namespace uTinyRipper.Classes.Shaders
{
	public enum Cull
	{
		Off		= 0,
		Front	= 1,
		Back	= 2,
		Count,

		Unknown	= -1,
	}

	public static class CullExtensions
	{
		public static bool IsOff(this Cull _this)
		{
			return _this == Cull.Off;
		}

		public static bool IsFront(this Cull _this)
		{
			return _this == Cull.Front;
		}

		public static bool IsBack(this Cull _this)
		{
			return _this == Cull.Back;
		}
	}
}
