using AssetRipper.Library.Configuration;

namespace AssetRipper.GUI.Components
{
	public class AudioExportConfigDropdown : BaseConfigurationDropdown<AudioExportFormat>
	{
		protected override string GetValueDisplayName(AudioExportFormat value) => value switch
		{
			AudioExportFormat.Native => "Raw",
			AudioExportFormat.Wav => "Convert to WAV",
			_ => base.GetValueDisplayName(value),
		};

		protected override string? GetValueDescription(AudioExportFormat value)  => value switch
		{
			AudioExportFormat.Native => "Raw FSB Audio. Cannot be imported into Unity, so only use this if you're an advanced user.",
			AudioExportFormat.Default => "Export assets as the content type embedded inside the FSB. Most audio types are exported as WAV, some are exported as OGG.",
			AudioExportFormat.Wav => "Convert all audio files to WAV files. Not recommended if importing into unity, as it may recompress files, causing a loss of quality.",
			_ => null,
		};
	}
}