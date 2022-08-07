namespace AssetRipper.Library.Configuration
{
	public enum SpriteExportMode
	{
		/// <summary>
		/// Export as yaml assets which can be viewed in the editor.
		/// This is the only mode that ensures a precise recovery of all metadata of sprites.
		/// <see href="https://github.com/trouger/AssetRipper/issues/2"/>
		/// </summary>
		Yaml,
		/// <summary>
		/// Export in the native asset format, where all sprites data are stored in texture importer settings.
		/// </summary>
		Native,
		/// <summary>
		/// Export as a Texture2D png image
		/// </summary>
		Texture2D,
	}
}
