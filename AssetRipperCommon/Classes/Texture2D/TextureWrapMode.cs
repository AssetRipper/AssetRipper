﻿namespace AssetRipper.Core.Classes.Texture2D
{
	/// <summary>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Graphics/GraphicsEnums.cs"/>
	/// </summary>
	public enum TextureWrapMode
	{
		/// <summary>
		/// Tiles the texture, creating a repeating pattern.
		/// </summary>
		Repeat = 0,
		/// <summary>
		/// Clamps the texture to the last pixel at the edge.
		/// </summary>
		Clamp = 1,
		/// <summary>
		/// Tiles the texture, creating a repeating pattern by mirroring it at every integer boundary.
		/// </summary>
		Mirror = 2,
		/// <summary>
		/// Mirrors the texture once, then clamps to edge pixels.
		/// </summary>
		MirrorOnce = 3
	}
}
