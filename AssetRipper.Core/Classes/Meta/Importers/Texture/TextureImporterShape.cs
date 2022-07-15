namespace AssetRipper.Core.Classes.Meta.Importers.Texture
{
	/// <summary>
	/// Select the kind of shape of your texture.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/AssetPipeline/TextureImporterEnums.cs"/>
	/// </summary>
	[Flags]
	public enum TextureImporterShape
	{
		/// <summary>
		/// Texture is 2D.
		/// </summary>
		Texture2D = 1,
		/// <summary>
		/// Texture is a Cubemap.
		/// </summary>
		TextureCube = 2,
		Texture2DArray = 4,
		Texture3D = 8,
	}
}
