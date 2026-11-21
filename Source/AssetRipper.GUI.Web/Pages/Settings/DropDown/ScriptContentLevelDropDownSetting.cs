using AssetRipper.Import.Configuration;

namespace AssetRipper.GUI.Web.Pages.Settings.DropDown;

public sealed class ScriptContentLevelDropDownSetting : DropDownSetting<ScriptContentLevel>
{
	public static ScriptContentLevelDropDownSetting Instance { get; } = new();

	public override string Title => Localization.ScriptContentLevelTitle;

	protected override string GetDisplayName(ScriptContentLevel value) => value switch
	{
		ScriptContentLevel.Level0 => Localization.ScriptContentLevel0,
		ScriptContentLevel.Level1 => Localization.ScriptContentLevel1,
		ScriptContentLevel.Level2 => Localization.ScriptContentLevel2,
		ScriptContentLevel.Level3 => Localization.ScriptContentLevel3,
		ScriptContentLevel.LevelIL2CPP => Localization.ScriptContentLevelIl2cpp,
		_ => base.GetDisplayName(value),
	};

	protected override string? GetDescription(ScriptContentLevel value) => value switch
	{
		ScriptContentLevel.Level0 => Localization.ScriptContentLevel0Description,
		ScriptContentLevel.Level1 => Localization.ScriptContentLevel1Description,
		ScriptContentLevel.Level2 => Localization.ScriptContentLevel2Description,
		ScriptContentLevel.Level3 => GameFileLoader.Premium
			? Localization.ScriptContentLevel3Description
			: Localization.NotAvailableInTheFreeEdition,
		ScriptContentLevel.LevelIL2CPP => Localization.ScriptContentLevelIl2cppDescription,
		_ => base.GetDescription(value),
	};
}
