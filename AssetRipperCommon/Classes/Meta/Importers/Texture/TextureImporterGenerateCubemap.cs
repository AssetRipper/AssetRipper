﻿namespace AssetRipper.Core.Classes.Meta.Importers.Texture
{
	/// <summary>
	/// Cubemap generation mode for TextureImporter.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/AssetPipeline/TextureImporterEnums.cs"/>
	/// </summary>
	public enum TextureImporterGenerateCubemap
	{
		/// <summary>
		/// Do not generate cubemap (default).
		/// </summary>
		None = 0,
		/// <summary>
		/// Generate cubemap from spheremap texture.
		/// </summary>
		Spheremap = 1,
		/// <summary>
		/// Generate cubemap from cylindrical texture.
		/// </summary>
		Cylindrical = 2,
		SimpleSpheremap = 3,
		NiceSpheremap = 4,
		/// <summary>
		/// Generate cubemap from vertical or horizontal cross texture.
		/// </summary>
		FullCubemap = 5,
		/// <summary>
		/// Automatically determine type of cubemap generation from the source image.
		/// </summary>
		AutoCubemap = 6,
	}
}
