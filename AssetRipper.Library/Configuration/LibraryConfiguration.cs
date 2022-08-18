using System.Collections.Generic;

using AssetRipper.Core.Configuration;
using AssetRipper.Core.Logging;

namespace AssetRipper.Library.Configuration
{
	public class LibraryConfiguration : CoreConfiguration
	{
		public override void ResetToDefaultValues()
		{
			base.ResetToDefaultValues();

			this.SetSetting<AudioExportFormat>(AudioExportFormat.Default);
			this.SetSetting<ImageExportFormat>(ImageExportFormat.Png);
			this.SetSetting<MeshExportFormat>(MeshExportFormat.Native);
			this.SetSetting<ScriptExportMode>(ScriptExportMode.Decompiled);
			this.SetSetting<ScriptLanguageVersion>(ScriptLanguageVersion.AutoSafe);
			this.SetSetting<ShaderExportMode>(ShaderExportMode.Dummy);
			this.SetSetting<SpriteExportMode>(SpriteExportMode.Yaml);
			this.SetSetting<TerrainExportMode>(TerrainExportMode.Yaml);
			this.SetSetting<TextExportMode>(TextExportMode.Parse);
		}

		public override void LogConfigurationValues()
		{
			base.LogConfigurationValues();

			Logger.Info(LogCategory.General, $"{nameof(AudioExportFormat)}: {this.GetSetting<AudioExportFormat>()}");
			Logger.Info(LogCategory.General, $"{nameof(ImageExportFormat)}: {this.GetSetting<ImageExportFormat>()}");
			Logger.Info(LogCategory.General, $"{nameof(MeshExportFormat)}: {this.GetSetting<MeshExportFormat>()}");
			Logger.Info(LogCategory.General, $"{nameof(ScriptExportMode)}: {this.GetSetting<ScriptExportMode>()}");
			Logger.Info(LogCategory.General, $"{nameof(ScriptLanguageVersion)}: {this.GetSetting<ScriptLanguageVersion>()}");
			Logger.Info(LogCategory.General, $"{nameof(ShaderExportMode)}: {this.GetSetting<ShaderExportMode>()}");
			Logger.Info(LogCategory.General, $"{nameof(SpriteExportMode)}: {this.GetSetting<SpriteExportMode>()}");
			Logger.Info(LogCategory.General, $"{nameof(TerrainExportMode)}: {this.GetSetting<TerrainExportMode>()}");
			Logger.Info(LogCategory.General, $"{nameof(TextExportMode)}: {this.GetSetting<TextExportMode>()}");
		}
	}
}
