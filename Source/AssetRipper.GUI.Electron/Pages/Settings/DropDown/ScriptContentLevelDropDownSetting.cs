﻿using AssetRipper.GUI.Localizations;
using AssetRipper.Import.Configuration;

namespace AssetRipper.GUI.Electron.Pages.Settings.DropDown;

public sealed class ScriptContentLevelDropDownSetting : DropDownSetting<ScriptContentLevel>
{
	public static ScriptContentLevelDropDownSetting Instance { get; } = new();

	public override string Title => Localization.ScriptContentLevelTitle;

	protected override string GetDisplayName(ScriptContentLevel value) => value switch
	{
		ScriptContentLevel.Level0 => Localization.ScriptContentLevel0,
		ScriptContentLevel.Level1 => Localization.ScriptContentLevel1,
		ScriptContentLevel.Level2 => Localization.ScriptContentLevel2,
		_ => base.GetDisplayName(value),
	};

	protected override string? GetDescription(ScriptContentLevel value) => value switch
	{
		ScriptContentLevel.Level0 => Localization.ScriptContentLevel0Description,
		ScriptContentLevel.Level1 => Localization.ScriptContentLevel1Description,
		ScriptContentLevel.Level2 => Localization.ScriptContentLevel2Description,
		_ => base.GetDescription(value),
	};
}
