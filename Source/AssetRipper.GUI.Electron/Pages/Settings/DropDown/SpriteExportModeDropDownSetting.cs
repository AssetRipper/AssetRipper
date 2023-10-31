﻿using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.GUI.Localizations;

namespace AssetRipper.GUI.Electron.Pages.Settings.DropDown;

public sealed class SpriteExportModeDropDownSetting : DropDownSetting<SpriteExportMode>
{
	public static SpriteExportModeDropDownSetting Instance { get; } = new();

	public override string Title => Localization.SpriteExportTitle;

	protected override string GetDisplayName(SpriteExportMode value) => value switch
	{
		SpriteExportMode.Yaml => Localization.SpriteFormatYaml,
		SpriteExportMode.Native => Localization.SpriteFormatNative,
		SpriteExportMode.Texture2D => Localization.SpriteFormatTexture,
		_ => base.GetDisplayName(value),
	};

	protected override string? GetDescription(SpriteExportMode value) => value switch
	{
		SpriteExportMode.Yaml => Localization.SpriteFormatYamlDescription,
		SpriteExportMode.Native => Localization.SpriteFormatNativeDescription,
		SpriteExportMode.Texture2D => Localization.SpriteFormatTextureDescription,
		_ => base.GetDescription(value),
	};
}
