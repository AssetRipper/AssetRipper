using System;

namespace uTinyRipper.Classes.Shaders
{
	[Flags]
	public enum ColorMask
	{
		None	= 0x0,
		A		= 0x1,
		R		= 0x2,
		G		= 0x4,
		B		= 0x8,

		RGBA	= R | G | B | A,
	}

	public static class ColorMaskExtensions
	{
		public static bool IsNone(this ColorMask _this)
		{
			return _this == ColorMask.None;
		}

		public static bool IsRed(this ColorMask _this)
		{
			return (_this & ColorMask.R) != 0;
		}
		
		public static bool IsGreen(this ColorMask _this)
		{
			return (_this & ColorMask.G) != 0;
		}

		public static bool IsBlue(this ColorMask _this)
		{
			return (_this & ColorMask.B) != 0;
		}

		public static bool IsAlpha(this ColorMask _this)
		{
			return (_this & ColorMask.A) != 0;
		}

		public static bool IsRBGA(this ColorMask _this)
		{
			return _this == ColorMask.RGBA;
		}
	}
}
