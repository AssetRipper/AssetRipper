namespace AssetRipper.Export.Configuration;

public enum TextExportMode
{
	/// <summary>
	/// Export as bytes
	/// </summary>
	Bytes,
	/// <summary>
	/// Export as plain text files
	/// </summary>
	Txt,
	/// <summary>
	/// Export as plain text files, but try to guess the file extension
	/// </summary>
	Parse,
}
