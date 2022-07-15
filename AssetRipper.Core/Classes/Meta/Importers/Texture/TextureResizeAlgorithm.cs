namespace AssetRipper.Core.Classes.Meta.Importers.Texture
{
	/// <summary>
	/// For Texture to be scaled down choose resize algorithm.
	/// ( Applyed only when Texture dimension is bigger than Max Size ).<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/AssetPipeline/TextureImporterEnums.cs"/>
	/// </summary>
	public enum TextureResizeAlgorithm
	{
		/// <summary>
		/// Default high quality resize algorithm.
		/// </summary>
		Mitchell = 0,
		/// <summary>
		/// Might provide better result than Mitchell for some noise textures preserving more sharp details.
		/// </summary>
		Bilinear = 1,
	}
}
