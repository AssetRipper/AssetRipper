using System.Collections.Generic;

using AssetRipper.Core.Configuration;
using AssetRipper.Core.Logging;

namespace AssetRipper.Library.Configuration
{
	public class LibraryConfiguration : CoreConfiguration
	{
		public Dictionary<Type, object> settings = new Dictionary<Type, object>();

		public T? GetSetting<T>()
		{
			object? result = this.settings.GetValueOrDefault(typeof(T));

			if (result == null || result == default)
				return default(T);

			return (T?)result;
		}

		public void SetSetting(Type type, object value)
		{
			if (value == null)
				return;

			this.settings.Remove(type);
			this.settings.Add(type, value);
		}

		public void SetSetting<T>(object value)
		{
			if (value == null)
				return;

			this.settings.Remove(typeof(T));
			this.settings.Add(typeof(T), value);
		}

		// NOTE: Is this actually a good idea? If this is called AFTER we set the default values,
		// the removed setting will not be restored to its default value, but it will just start
		// returning null.
		public void RemoveSetting<T>()
		{
			this.settings.Remove(typeof(T));
		}

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
