// Auto-generated code. Do not modify manually.

using AssetRipper.Export.Configuration;
using AssetRipper.GUI.Web.Pages.Settings.DropDown;
using AssetRipper.Import.Configuration;
using AssetRipper.Processing.Configuration;

namespace AssetRipper.GUI.Web.Pages.Settings;

#nullable enable

partial class SettingsPage
{
	private static void SetProperty(string key, string? value)
	{
		switch (key)
		{
			case nameof(ImportSettings.ScriptContentLevel):
				Configuration.ImportSettings.ScriptContentLevel = TryParseEnum<ScriptContentLevel>(value);
				break;
			case nameof(ImportSettings.StreamingAssetsMode):
				Configuration.ImportSettings.StreamingAssetsMode = TryParseEnum<StreamingAssetsMode>(value);
				break;
			case nameof(ImportSettings.DefaultVersion):
				Configuration.ImportSettings.DefaultVersion = TryParseUnityVersion(value);
				break;
			case nameof(ImportSettings.TargetVersion):
				Configuration.ImportSettings.TargetVersion = TryParseUnityVersion(value);
				break;
			case nameof(ProcessingSettings.BundledAssetsExportMode):
				Configuration.ProcessingSettings.BundledAssetsExportMode = TryParseEnum<BundledAssetsExportMode>(value);
				break;
			case nameof(ExportSettings.AudioExportFormat):
				Configuration.ExportSettings.AudioExportFormat = TryParseEnum<AudioExportFormat>(value);
				break;
			case nameof(ExportSettings.ImageExportFormat):
				Configuration.ExportSettings.ImageExportFormat = TryParseEnum<ImageExportFormat>(value);
				break;
			case nameof(ExportSettings.LightmapTextureExportFormat):
				Configuration.ExportSettings.LightmapTextureExportFormat = TryParseEnum<LightmapTextureExportFormat>(value);
				break;
			case nameof(ExportSettings.ScriptExportMode):
				Configuration.ExportSettings.ScriptExportMode = TryParseEnum<ScriptExportMode>(value);
				break;
			case nameof(ExportSettings.ScriptLanguageVersion):
				Configuration.ExportSettings.ScriptLanguageVersion = TryParseEnum<ScriptLanguageVersion>(value);
				break;
			case nameof(ExportSettings.ShaderExportMode):
				Configuration.ExportSettings.ShaderExportMode = TryParseEnum<ShaderExportMode>(value);
				break;
			case nameof(ExportSettings.SpriteExportMode):
				Configuration.ExportSettings.SpriteExportMode = TryParseEnum<SpriteExportMode>(value);
				break;
			case nameof(ExportSettings.TextExportMode):
				Configuration.ExportSettings.TextExportMode = TryParseEnum<TextExportMode>(value);
				break;
			case nameof(ExportSettings.LanguageCode):
				Configuration.ExportSettings.LanguageCode = value;
				break;
		}
	}

	private static readonly Dictionary<string, Action<bool>> booleanProperties = new()
	{
		{ nameof(ImportSettings.IgnoreStreamingAssets), (value) => { Configuration.ImportSettings.IgnoreStreamingAssets = value; } },
		{ nameof(ProcessingSettings.EnablePrefabOutlining), (value) => { Configuration.ProcessingSettings.EnablePrefabOutlining = value; } },
		{ nameof(ProcessingSettings.EnableStaticMeshSeparation), (value) => { Configuration.ProcessingSettings.EnableStaticMeshSeparation = value; } },
		{ nameof(ProcessingSettings.EnableAssetDeduplication), (value) => { Configuration.ProcessingSettings.EnableAssetDeduplication = value; } },
		{ nameof(ProcessingSettings.RemoveNullableAttributes), (value) => { Configuration.ProcessingSettings.RemoveNullableAttributes = value; } },
		{ nameof(ProcessingSettings.PublicizeAssemblies), (value) => { Configuration.ProcessingSettings.PublicizeAssemblies = value; } },
		{ nameof(ExportSettings.ScriptTypesFullyQualified), (value) => { Configuration.ExportSettings.ScriptTypesFullyQualified = value; } },
		{ nameof(ExportSettings.ExportUnreadableAssets), (value) => { Configuration.ExportSettings.ExportUnreadableAssets = value; } },
		{ nameof(ExportSettings.SaveSettingsToDisk), (value) => { Configuration.ExportSettings.SaveSettingsToDisk = value; } },
	};

	private static void WriteDropDownForScriptContentLevel(TextWriter writer)
	{
		WriteDropDown(writer, ScriptContentLevelDropDownSetting.Instance, Configuration.ImportSettings.ScriptContentLevel, nameof(ImportSettings.ScriptContentLevel));
	}

	private static void WriteCheckBoxForIgnoreStreamingAssets(TextWriter writer, string label, bool disabled = false)
	{
		WriteCheckBox(writer, label, Configuration.ImportSettings.IgnoreStreamingAssets, nameof(ImportSettings.IgnoreStreamingAssets), disabled);
	}

	private static void WriteDropDownForStreamingAssetsMode(TextWriter writer)
	{
		WriteDropDown(writer, StreamingAssetsModeDropDownSetting.Instance, Configuration.ImportSettings.StreamingAssetsMode, nameof(ImportSettings.StreamingAssetsMode));
	}

	private static void WriteCheckBoxForEnablePrefabOutlining(TextWriter writer, string label, bool disabled = false)
	{
		WriteCheckBox(writer, label, Configuration.ProcessingSettings.EnablePrefabOutlining, nameof(ProcessingSettings.EnablePrefabOutlining), disabled);
	}

	private static void WriteCheckBoxForEnableStaticMeshSeparation(TextWriter writer, string label, bool disabled = false)
	{
		WriteCheckBox(writer, label, Configuration.ProcessingSettings.EnableStaticMeshSeparation, nameof(ProcessingSettings.EnableStaticMeshSeparation), disabled);
	}

	private static void WriteCheckBoxForEnableAssetDeduplication(TextWriter writer, string label, bool disabled = false)
	{
		WriteCheckBox(writer, label, Configuration.ProcessingSettings.EnableAssetDeduplication, nameof(ProcessingSettings.EnableAssetDeduplication), disabled);
	}

	private static void WriteCheckBoxForRemoveNullableAttributes(TextWriter writer, string label, bool disabled = false)
	{
		WriteCheckBox(writer, label, Configuration.ProcessingSettings.RemoveNullableAttributes, nameof(ProcessingSettings.RemoveNullableAttributes), disabled);
	}

	private static void WriteCheckBoxForPublicizeAssemblies(TextWriter writer, string label, bool disabled = false)
	{
		WriteCheckBox(writer, label, Configuration.ProcessingSettings.PublicizeAssemblies, nameof(ProcessingSettings.PublicizeAssemblies), disabled);
	}

	private static void WriteDropDownForBundledAssetsExportMode(TextWriter writer)
	{
		WriteDropDown(writer, BundledAssetsExportModeDropDownSetting.Instance, Configuration.ProcessingSettings.BundledAssetsExportMode, nameof(ProcessingSettings.BundledAssetsExportMode));
	}

	private static void WriteDropDownForAudioExportFormat(TextWriter writer)
	{
		WriteDropDown(writer, AudioExportFormatDropDownSetting.Instance, Configuration.ExportSettings.AudioExportFormat, nameof(ExportSettings.AudioExportFormat));
	}

	private static void WriteDropDownForImageExportFormat(TextWriter writer)
	{
		WriteDropDown(writer, ImageExportFormatDropDownSetting.Instance, Configuration.ExportSettings.ImageExportFormat, nameof(ExportSettings.ImageExportFormat));
	}

	private static void WriteDropDownForLightmapTextureExportFormat(TextWriter writer)
	{
		WriteDropDown(writer, LightmapTextureExportFormatDropDownSetting.Instance, Configuration.ExportSettings.LightmapTextureExportFormat, nameof(ExportSettings.LightmapTextureExportFormat));
	}

	private static void WriteDropDownForScriptExportMode(TextWriter writer)
	{
		WriteDropDown(writer, ScriptExportModeDropDownSetting.Instance, Configuration.ExportSettings.ScriptExportMode, nameof(ExportSettings.ScriptExportMode));
	}

	private static void WriteDropDownForScriptLanguageVersion(TextWriter writer)
	{
		WriteDropDown(writer, ScriptLanguageVersionDropDownSetting.Instance, Configuration.ExportSettings.ScriptLanguageVersion, nameof(ExportSettings.ScriptLanguageVersion));
	}

	private static void WriteCheckBoxForScriptTypesFullyQualified(TextWriter writer, string label, bool disabled = false)
	{
		WriteCheckBox(writer, label, Configuration.ExportSettings.ScriptTypesFullyQualified, nameof(ExportSettings.ScriptTypesFullyQualified), disabled);
	}

	private static void WriteDropDownForShaderExportMode(TextWriter writer)
	{
		WriteDropDown(writer, ShaderExportModeDropDownSetting.Instance, Configuration.ExportSettings.ShaderExportMode, nameof(ExportSettings.ShaderExportMode));
	}

	private static void WriteDropDownForSpriteExportMode(TextWriter writer)
	{
		WriteDropDown(writer, SpriteExportModeDropDownSetting.Instance, Configuration.ExportSettings.SpriteExportMode, nameof(ExportSettings.SpriteExportMode));
	}

	private static void WriteDropDownForTextExportMode(TextWriter writer)
	{
		WriteDropDown(writer, TextExportModeDropDownSetting.Instance, Configuration.ExportSettings.TextExportMode, nameof(ExportSettings.TextExportMode));
	}

	private static void WriteCheckBoxForExportUnreadableAssets(TextWriter writer, string label, bool disabled = false)
	{
		WriteCheckBox(writer, label, Configuration.ExportSettings.ExportUnreadableAssets, nameof(ExportSettings.ExportUnreadableAssets), disabled);
	}

	private static void WriteCheckBoxForSaveSettingsToDisk(TextWriter writer, string label, bool disabled = false)
	{
		WriteCheckBox(writer, label, Configuration.ExportSettings.SaveSettingsToDisk, nameof(ExportSettings.SaveSettingsToDisk), disabled);
	}
}
