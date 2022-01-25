using AssetRipper.Library.Configuration;

namespace AssetRipper.GUI.Components
{
	public class AudioExportConfigDropdown : BaseConfigurationDropdown<AudioExportFormat>
	{
		protected override string GetValueDisplayName(AudioExportFormat value) => value switch
		{
			AudioExportFormat.Native => MainWindow.Instance.LocalizationManager["audio_format_native"],
			AudioExportFormat.Wav => MainWindow.Instance.LocalizationManager["audio_format_force_wav"],
			AudioExportFormat.Mp3 => MainWindow.Instance.LocalizationManager["audio_format_force_mp3"],
			AudioExportFormat.Default => MainWindow.Instance.LocalizationManager["audio_format_default"],
			_ => base.GetValueDisplayName(value),
		};

		protected override string? GetValueDescription(AudioExportFormat value) => value switch
		{
			AudioExportFormat.Native => MainWindow.Instance.LocalizationManager["audio_format_native_description"],
			AudioExportFormat.Wav => MainWindow.Instance.LocalizationManager["audio_format_force_wav_description"],
			AudioExportFormat.Mp3 => MainWindow.Instance.LocalizationManager["audio_format_force_mp3_description"],
			AudioExportFormat.Default => MainWindow.Instance.LocalizationManager["audio_format_default_description"],
			_ => null,
		};
	}
}