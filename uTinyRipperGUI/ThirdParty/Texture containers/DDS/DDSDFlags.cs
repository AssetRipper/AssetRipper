using System;

namespace uTinyRipperGUI.TextureContainers.DDS
{
	[Flags]
	public enum DDSDFlags : uint
	{
		/// <summary>
		/// Required in every .dds file.
		/// </summary>
		CAPS = 0x1,
		/// <summary>
		/// Required in every .dds file.
		/// </summary>
		HEIGHT = 0x2,
		/// <summary>
		/// Required in every .dds file.
		/// </summary>
		WIDTH = 0x4,
		/// <summary>
		/// Required when pitch is provided for an uncompressed texture.
		/// </summary>
		PITCH = 0x8,
		/// <summary>
		/// Required in every .dds file.
		/// </summary>
		PIXELFORMAT = 0x1000,
		/// <summary>
		/// Required in a mipmapped texture.
		/// </summary>
		MIPMAPCOUNT = 0x20000,
		/// <summary>
		/// Required when pitch is provided for a compressed texture.
		/// </summary>
		LINEARSIZE = 0x80000,
		/// <summary>
		/// Required in a depth texture.
		/// </summary>
		DEPTH = 0x800000,
	}

	public static class DDSDFlagsExtensions
	{
		public static bool IsCaps(this DDSDFlags _this)
		{
			return (_this & DDSDFlags.CAPS) != 0;
		}

		public static bool IsHeight(this DDSDFlags _this)
		{
			return (_this & DDSDFlags.HEIGHT) != 0;
		}

		public static bool IsWidth(this DDSDFlags _this)
		{
			return (_this & DDSDFlags.WIDTH) != 0;
		}

		public static bool IsPitch(this DDSDFlags _this)
		{
			return (_this & DDSDFlags.PITCH) != 0;
		}

		public static bool IsPixelFormat(this DDSDFlags _this)
		{
			return (_this & DDSDFlags.PIXELFORMAT) != 0;
		}

		public static bool IsMipMapCount(this DDSDFlags _this)
		{
			return (_this & DDSDFlags.MIPMAPCOUNT) != 0;
		}

		public static bool IsLinearSize(this DDSDFlags _this)
		{
			return (_this & DDSDFlags.LINEARSIZE) != 0;
		}

		public static bool IsDepth(this DDSDFlags _this)
		{
			return (_this & DDSDFlags.DEPTH) != 0;
		}
	}
}
