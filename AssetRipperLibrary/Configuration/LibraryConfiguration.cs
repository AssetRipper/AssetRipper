using AssetRipper.Core.Configuration;
using AssetRipper.Core.Logging;

namespace AssetRipper.Library.Configuration
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
		/// The coordinate space that meshes get exported in for non-yaml export.
		/// </summary>
		public MeshCoordinateSpace MeshCoordinateSpace { get; set; }
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

		public override void ResetToDefaultValues()
		{
			base.ResetToDefaultValues();
			AudioExportFormat = AudioExportFormat.Default;
			ImageExportFormat = ImageExportFormat.Png;
			MeshCoordinateSpace = MeshCoordinateSpace.Unity;
			MeshExportFormat = MeshExportFormat.Native;
			ScriptExportMode = ScriptExportMode.Decompiled;
			ScriptLanguageVersion = ScriptLanguageVersion.AutoSafe;
			ShaderExportMode = ShaderExportMode.Dummy;
			SpriteExportMode = SpriteExportMode.Native;
			TerrainExportMode = TerrainExportMode.Native;
			TextExportMode = TextExportMode.Parse;
		}

		public override void LogConfigurationValues()
		{
			base.LogConfigurationValues();
			Logger.Info(LogCategory.General, $"{nameof(AudioExportFormat)}: {AudioExportFormat}");
			Logger.Info(LogCategory.General, $"{nameof(ImageExportFormat)}: {ImageExportFormat}");
			Logger.Info(LogCategory.General, $"{nameof(MeshCoordinateSpace)}: {MeshCoordinateSpace}");
			Logger.Info(LogCategory.General, $"{nameof(MeshExportFormat)}: {MeshExportFormat}");
			Logger.Info(LogCategory.General, $"{nameof(ScriptExportMode)}: {ScriptExportMode}");
			Logger.Info(LogCategory.General, $"{nameof(ScriptLanguageVersion)}: {ScriptLanguageVersion}");
			Logger.Info(LogCategory.General, $"{nameof(ShaderExportMode)}: {ShaderExportMode}");
			Logger.Info(LogCategory.General, $"{nameof(SpriteExportMode)}: {SpriteExportMode}");
			Logger.Info(LogCategory.General, $"{nameof(TerrainExportMode)}: {TerrainExportMode}");
			Logger.Info(LogCategory.General, $"{nameof(TextExportMode)}: {TextExportMode}");
		}
	}
}
