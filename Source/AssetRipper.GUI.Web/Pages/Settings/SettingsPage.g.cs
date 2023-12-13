// Auto-generated code. Do not modify manually.

using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.GUI.Web.Pages.Settings.DropDown;
using AssetRipper.Import.Configuration;

namespace AssetRipper.GUI.Web.Pages.Settings;

#nullable enable

partial class SettingsPage
{
	private static void SetProperty(string key, string? value)
	{
		switch (key)
		{
			case nameof(Configuration.DefaultVersion):
				Configuration.DefaultVersion = TryParseUnityVersion(value);
				break;
			case nameof(Configuration.AudioExportFormat):
				Configuration.AudioExportFormat = TryParseEnum<AudioExportFormat>(value);
				break;
			case nameof(Configuration.BundledAssetsExportMode):
				Configuration.BundledAssetsExportMode = TryParseEnum<BundledAssetsExportMode>(value);
				break;
			case nameof(Configuration.ImageExportFormat):
				Configuration.ImageExportFormat = TryParseEnum<ImageExportFormat>(value);
				break;
			case nameof(Configuration.MeshExportFormat):
				Configuration.MeshExportFormat = TryParseEnum<MeshExportFormat>(value);
				break;
			case nameof(Configuration.SpriteExportMode):
				Configuration.SpriteExportMode = TryParseEnum<SpriteExportMode>(value);
				break;
			case nameof(Configuration.TerrainExportMode):
				Configuration.TerrainExportMode = TryParseEnum<TerrainExportMode>(value);
				break;
			case nameof(Configuration.TextExportMode):
				Configuration.TextExportMode = TryParseEnum<TextExportMode>(value);
				break;
			case nameof(Configuration.ShaderExportMode):
				Configuration.ShaderExportMode = TryParseEnum<ShaderExportMode>(value);
				break;
			case nameof(Configuration.ScriptExportMode):
				Configuration.ScriptExportMode = TryParseEnum<ScriptExportMode>(value);
				break;
			case nameof(Configuration.ScriptContentLevel):
				Configuration.ScriptContentLevel = TryParseEnum<ScriptContentLevel>(value);
				break;
			case nameof(Configuration.ScriptLanguageVersion):
				Configuration.ScriptLanguageVersion = TryParseEnum<ScriptLanguageVersion>(value);
				break;
		}
	}

	private static readonly Dictionary<string, Action<bool>> booleanProperties = new()
	{
		{ nameof(Configuration.EnablePrefabOutlining), (value) => { Configuration.EnablePrefabOutlining = value; } },
		{ nameof(Configuration.IgnoreStreamingAssets), (value) => { Configuration.IgnoreStreamingAssets = value; } },
		{ nameof(Configuration.IgnoreEngineAssets), (value) => { Configuration.IgnoreEngineAssets = value; } },
	};

	private static void WriteCheckBoxForEnablePrefabOutlining(TextWriter writer, string label)
	{
		WriteCheckBox(writer, label, Configuration.EnablePrefabOutlining, nameof(Configuration.EnablePrefabOutlining));
	}

	private static void WriteCheckBoxForIgnoreStreamingAssets(TextWriter writer, string label)
	{
		WriteCheckBox(writer, label, Configuration.IgnoreStreamingAssets, nameof(Configuration.IgnoreStreamingAssets));
	}

	private static void WriteCheckBoxForIgnoreEngineAssets(TextWriter writer, string label)
	{
		WriteCheckBox(writer, label, Configuration.IgnoreEngineAssets, nameof(Configuration.IgnoreEngineAssets));
	}

	private static void WriteDropDownForAudioExportFormat(TextWriter writer)
	{
		WriteDropDown(writer, AudioExportFormatDropDownSetting.Instance, Configuration.AudioExportFormat, nameof(Configuration.AudioExportFormat));
	}

	private static void WriteDropDownForBundledAssetsExportMode(TextWriter writer)
	{
		WriteDropDown(writer, BundledAssetsExportModeDropDownSetting.Instance, Configuration.BundledAssetsExportMode, nameof(Configuration.BundledAssetsExportMode));
	}

	private static void WriteDropDownForImageExportFormat(TextWriter writer)
	{
		WriteDropDown(writer, ImageExportFormatDropDownSetting.Instance, Configuration.ImageExportFormat, nameof(Configuration.ImageExportFormat));
	}

	private static void WriteDropDownForMeshExportFormat(TextWriter writer)
	{
		WriteDropDown(writer, MeshExportFormatDropDownSetting.Instance, Configuration.MeshExportFormat, nameof(Configuration.MeshExportFormat));
	}

	private static void WriteDropDownForSpriteExportMode(TextWriter writer)
	{
		WriteDropDown(writer, SpriteExportModeDropDownSetting.Instance, Configuration.SpriteExportMode, nameof(Configuration.SpriteExportMode));
	}

	private static void WriteDropDownForTerrainExportMode(TextWriter writer)
	{
		WriteDropDown(writer, TerrainExportModeDropDownSetting.Instance, Configuration.TerrainExportMode, nameof(Configuration.TerrainExportMode));
	}

	private static void WriteDropDownForTextExportMode(TextWriter writer)
	{
		WriteDropDown(writer, TextExportModeDropDownSetting.Instance, Configuration.TextExportMode, nameof(Configuration.TextExportMode));
	}

	private static void WriteDropDownForShaderExportMode(TextWriter writer)
	{
		WriteDropDown(writer, ShaderExportModeDropDownSetting.Instance, Configuration.ShaderExportMode, nameof(Configuration.ShaderExportMode));
	}

	private static void WriteDropDownForScriptExportMode(TextWriter writer)
	{
		WriteDropDown(writer, ScriptExportModeDropDownSetting.Instance, Configuration.ScriptExportMode, nameof(Configuration.ScriptExportMode));
	}

	private static void WriteDropDownForScriptContentLevel(TextWriter writer)
	{
		WriteDropDown(writer, ScriptContentLevelDropDownSetting.Instance, Configuration.ScriptContentLevel, nameof(Configuration.ScriptContentLevel));
	}

	private static void WriteDropDownForScriptLanguageVersion(TextWriter writer)
	{
		WriteDropDown(writer, ScriptLanguageVersionDropDownSetting.Instance, Configuration.ScriptLanguageVersion, nameof(Configuration.ScriptLanguageVersion));
	}
}
