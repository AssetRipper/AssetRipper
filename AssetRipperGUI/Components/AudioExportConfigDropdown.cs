using AssetRipper.Library.Configuration;

namespace AssetRipper.GUI.Components
{
	public class AudioExportConfigDropdown : BaseConfigurationDropdown<AudioExportFormat>
	{
		protected override string GetValueDisplayName(AudioExportFormat value) => value switch
		{
			AudioExportFormat.Native => MainWindow.Instance.LanguageManager["audio_format_native"],
			AudioExportFormat.Wav => MainWindow.Instance.LanguageManager["audio_format_force_wav"],
			AudioExportFormat.Default => MainWindow.Instance.LanguageManager["audio_format_default"],
			_ => base.GetValueDisplayName(value),
		};

		protected override string? GetValueDescription(AudioExportFormat value)  => value switch
		{
			AudioExportFormat.Native => MainWindow.Instance.LanguageManager["audio_format_native_description"],
			AudioExportFormat.Wav => MainWindow.Instance.LanguageManager["audio_format_force_wav_description"],
			AudioExportFormat.Default => MainWindow.Instance.LanguageManager["audio_format_default_description"],
			_ => null,
		};
	}
}