namespace AssetRipper.Library.Configuration
{
	public enum AudioExportFormat
	{
		/// <summary>
		/// A native exporter usually in the FSB (fmod sound bank) format. Not very usable.
		/// </summary>
		Native,
		/// <summary>
		/// The compression used by FMOD. This is the recommended audio output format.
		/// </summary>
		Ogg,
		/// <summary>
		/// A common lossless audio format. Not advised if rebundling.
		/// </summary>
		Wav,
	}
}