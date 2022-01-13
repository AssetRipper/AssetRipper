using AssetRipper.Core.Configuration;
using AssetRipper.Core.Logging;
using ICSharpCode.Decompiler.CSharp;

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
		public LanguageVersion ScriptLanguageVersion { get; set; }
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
			ScriptLanguageVersion = LanguageVersion.CSharp7_3;
			SpriteExportMode = SpriteExportMode.Native;
			TerrainExportMode = TerrainExportMode.Native;
			TextExportMode = TextExportMode.Parse;
		}

		public override void LogConfigurationValues()
		{
			base.LogConfigurationValues();
			Logger.Info(LogCategory.General, $"AudioExportFormat: {AudioExportFormat}");
			Logger.Info(LogCategory.General, $"ImageExportFormat: {ImageExportFormat}");
			Logger.Info(LogCategory.General, $"MeshCoordinateSpace: {MeshCoordinateSpace}");
			Logger.Info(LogCategory.General, $"MeshExportFormat: {MeshExportFormat}");
			Logger.Info(LogCategory.General, $"ScriptExportMode: {ScriptExportMode}");
			Logger.Info(LogCategory.General, $"ScriptLanguageVersion: {ScriptLanguageVersion}");
			Logger.Info(LogCategory.General, $"SpriteExportMode: {SpriteExportMode}");
			Logger.Info(LogCategory.General, $"TerrainExportMode: {TerrainExportMode}");
			Logger.Info(LogCategory.General, $"TextExportMode: {TextExportMode}");
		}
	}
}
