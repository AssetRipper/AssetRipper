using System.Collections.Generic;

using AssetRipper.Core.Configuration;
using AssetRipper.Core.Logging;

namespace AssetRipper.Library.Configuration
{
	public class LibraryConfiguration : CoreConfiguration
	{
		public Dictionary<Type, object> settings = new Dictionary<Type, object>();

		/*
		/// <summary>
		/// The file format that audio clips get exported in. Recommended: Ogg
		/// </summary>
		public AudioExportFormat AudioExportFormat { get; set; }
		/// <summary>
		/// The file format that images (like textures) get exported in.
		/// </summary>
		public ImageExportFormat ImageExportFormat { get; set; }
		/// <summary>
		/// The format that meshes get exported in. Recommended: Native
		/// </summary>
		public MeshExportFormat MeshExportFormat { get; set; }
		/// <summary>
		/// How are MonoScripts exported? Recommended: Decompiled
		/// </summary>
		public ScriptExportMode ScriptExportMode { get; set; }
		/// <summary>
		/// The C# language version of decompiled scripts.
		/// </summary>
		public ScriptLanguageVersion ScriptLanguageVersion { get; set; }
		/// <summary>
		/// How to export shaders?
		/// </summary>
		public ShaderExportMode ShaderExportMode { get; set; }
		/// <summary>
		/// Should sprites be exported as a texture? Recommended: Native
		/// </summary>
		public SpriteExportMode SpriteExportMode { get; set; }
		/// <summary>
		/// How terrain data is exported. Recommended: Native
		/// </summary>
		public TerrainExportMode TerrainExportMode { get; set; }
		/// <summary>
		/// How are text assets exported?
		/// </summary>
		public TextExportMode TextExportMode { get; set; }
		*/

		public T? GetSetting<T>()
		{
			this.settings.TryGetValue(typeof(T), out object? result);

			return (T?)result;
		}

		public void SetSetting<T>(object value)
		{
			this.settings.TryAdd(typeof(T), value);
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
