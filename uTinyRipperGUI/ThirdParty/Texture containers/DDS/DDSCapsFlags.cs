using System;

namespace uTinyRipperGUI.TextureContainers.DDS
{
	[Flags]
	public enum DDSCapsFlags : uint
	{
		/// <summary>
		/// Optional; must be used on any file that contains more than one surface
		/// (a mipmap, a cubic environment map, or mipmapped volume texture).
		/// </summary>
		DDSCAPS_COMPLEX = 0x8,
		/// <summary>
		/// Required
		/// </summary>
		DDSCAPS_TEXTURE = 0x1000,
		/// <summary>
		/// Optional; should be used for a mipmap.
		/// </summary>
		DDSCAPS_MIPMAP = 0x400000,
	}
}
