namespace AssetRipper.Export.Configuration;

public enum AudioExportFormat
{
	/// <summary>
	/// Export as a yaml asset and resS file. This is a safe option and is the backup when things go wrong.
	/// </summary>
	Yaml,
	/// <summary>
	/// For advanced users. This exports in a native format, usually FSB (FMOD Sound Bank). FSB files cannot be used in Unity Editor.
	/// </summary>
	Native,
	/// <summary>
	/// This is the recommended option. Audio assets are exported in the compression of the source, usually OGG.
	/// </summary>
	Default,
	/// <summary>
	/// Not advised if rebundling. This converts audio to the WAV format when possible
	/// </summary>
	PreferWav,
}
