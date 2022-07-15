namespace AssetRipper.Core.Classes.Meta.Importers.Texture
{
	/// <summary>
	/// Normal map filtering mode for TextureImporter.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/AssetPipeline/TextureImporterEnums.cs"/>
	/// </summary>
	public enum TextureImporterNormalFilter
	{
		/// <summary>
		/// Standard normal map filter.
		/// </summary>
		Standard,
		/// <summary>
		/// Sobel normal map filter.
		/// </summary>
		Sobel
	}
}
