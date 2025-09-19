namespace AssetRipper.Export.Configuration;

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
	/// <remarks>
	/// The output from this mode was substantially changed by
	/// <see href="https://github.com/AssetRipper/AssetRipper/commit/084b3e5ea7826ac2f54ed2b11cbfbbf3692ddc9c"/>.
	/// Using this is inadvisable.
	/// </remarks>
	Native,
	/// <summary>
	/// Export as a Texture2D png image
	/// </summary>
	/// <remarks>
	/// The output from this mode was substantially changed by
	/// <see href="https://github.com/AssetRipper/AssetRipper/commit/084b3e5ea7826ac2f54ed2b11cbfbbf3692ddc9c"/>.
	/// Using this is inadvisable.
	/// </remarks>
	Texture2D,
}
