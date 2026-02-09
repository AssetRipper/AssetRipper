using AssetRipper.SourceGenerated.Subclasses.CrashReportingSettings;

namespace AssetRipper.SourceGenerated.Extensions;

public static class CrashReportingSettingsExtensions
{
	public static void ConvertToEditorFormat(this ICrashReportingSettings settings)
	{
		if (settings.Has_NativeEventUrl() && settings.NativeEventUrl.Data.Length == 0)
		{
			settings.NativeEventUrl = "https://perf-events.cloud.unity3d.com/symbolicate"; //not sure where this url came from
		}

		// NOTE: editor has different value than player
		settings.LogBufferSize = 10;

		settings.CaptureEditorExceptions = true;
	}
}
