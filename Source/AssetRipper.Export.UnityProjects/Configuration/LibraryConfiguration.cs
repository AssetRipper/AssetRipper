using AssetRipper.Import.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Mining.PredefinedAssets;
using System.Diagnostics;

namespace AssetRipper.Export.UnityProjects.Configuration
{
	public class LibraryConfiguration : CoreConfiguration
	{
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
		public bool EnablePrefabOutlining { get; set; }
		public SingletonDataStorage SingletonData { get; } = new();
		public ListDataStorage ListData { get; } = new();

		public LibraryConfiguration()
		{
			AddDebugData();
			SingletonData.RegisterKey(nameof(EngineResourceData));
		}

		public override void ResetToDefaultValues()
		{
			base.ResetToDefaultValues();
			AudioExportFormat = AudioExportFormat.Default;
			ImageExportFormat = ImageExportFormat.Png;
			MeshExportFormat = MeshExportFormat.Native;
			ScriptExportMode = ScriptExportMode.Hybrid;
			ScriptLanguageVersion = ScriptLanguageVersion.AutoSafe;
			ShaderExportMode = ShaderExportMode.Dummy;
			SpriteExportMode = SpriteExportMode.Yaml;
			TerrainExportMode = TerrainExportMode.Yaml;
			TextExportMode = TextExportMode.Parse;
			EnablePrefabOutlining = false;
			SingletonData.Clear();
			ListData.Clear();
		}

		public override void LogConfigurationValues()
		{
			base.LogConfigurationValues();
			Logger.Info(LogCategory.General, $"{nameof(AudioExportFormat)}: {AudioExportFormat}");
			Logger.Info(LogCategory.General, $"{nameof(ImageExportFormat)}: {ImageExportFormat}");
			Logger.Info(LogCategory.General, $"{nameof(MeshExportFormat)}: {MeshExportFormat}");
			Logger.Info(LogCategory.General, $"{nameof(ScriptExportMode)}: {ScriptExportMode}");
			Logger.Info(LogCategory.General, $"{nameof(ScriptLanguageVersion)}: {ScriptLanguageVersion}");
			Logger.Info(LogCategory.General, $"{nameof(ShaderExportMode)}: {ShaderExportMode}");
			Logger.Info(LogCategory.General, $"{nameof(SpriteExportMode)}: {SpriteExportMode}");
			Logger.Info(LogCategory.General, $"{nameof(TerrainExportMode)}: {TerrainExportMode}");
			Logger.Info(LogCategory.General, $"{nameof(TextExportMode)}: {TextExportMode}");
			Logger.Info(LogCategory.General, $"{nameof(EnablePrefabOutlining)}: {EnablePrefabOutlining}");
		}

		[Conditional("DEBUG")]
		private void AddDebugData()
		{
			SingletonData["README"] = "This is a singleton entry. It is used to store information that can be contained in a single file.";
			ListData["README"] = ["This is a list entry. It is used to store information that might be contained in multiple files."];
			ListData["Fibonacci"] = ["1", "1", "2", "3", "5", "8", "13", "21", "34", "55"];
			ListData.RegisterKey("Unused Key");
		}
	}
}
