namespace AssetRipper.GUI.Electron.Pages.Settings.DropDown;

public abstract class DropDownSetting
{
	public abstract string Title { get; }
	public abstract IReadOnlyList<DropDownItem> GetValues();
}
public class DropDownSetting<T> : DropDownSetting where T : struct, Enum
{
	public override string Title => typeof(T).Name;

	protected virtual IReadOnlyList<T> Values { get; } = Enum.GetValues<T>();

	protected virtual string GetDisplayName(T value) => Enum.GetName(value) ?? value.ToString();

	protected virtual string? GetDescription(T value) => null;

	public sealed override IReadOnlyList<DropDownItem> GetValues()
	{
		DropDownItem[] result = new DropDownItem[Values.Count];
		for (int i = 0; i < Values.Count; i++)
		{
			T value = Values[i];
			result[i] = new(value.ToString(), GetDisplayName(value), GetDescription(value));
		}
		return result;
	}
}
