using System;

namespace uTinyRipper.Classes.Shaders
{
	[Flags]
	public enum SerializedPropertyFlag : uint
	{
		HideInInspector		= 0x1,
		PerRendererData		= 0x2,
		NoScaleOffset		= 0x4,
		Normal				= 0x8,
		HDR					= 0x10,
		Gamma				= 0x20,
	}

	public static class SerializedPropertyFlagExtensions
	{
		public static bool IsHideInEnspector(this SerializedPropertyFlag _this)
		{
			return (_this & SerializedPropertyFlag.HideInInspector) != 0;
		}

		public static bool IsPerRendererData(this SerializedPropertyFlag _this)
		{
			return (_this & SerializedPropertyFlag.PerRendererData) != 0;
		}

		public static bool IsNoScaleOffset(this SerializedPropertyFlag _this)
		{
			return (_this & SerializedPropertyFlag.NoScaleOffset) != 0;
		}

		public static bool IsNormal(this SerializedPropertyFlag _this)
		{
			return (_this & SerializedPropertyFlag.Normal) != 0;
		}

		public static bool IsHDR(this SerializedPropertyFlag _this)
		{
			return (_this & SerializedPropertyFlag.HDR) != 0;
		}

		public static bool IsGamma(this SerializedPropertyFlag _this)
		{
			return (_this & SerializedPropertyFlag.Gamma) != 0;
		}
	}
}
