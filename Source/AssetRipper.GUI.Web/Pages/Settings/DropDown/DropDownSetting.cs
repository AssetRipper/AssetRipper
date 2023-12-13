namespace AssetRipper.GUI.Web.Pages.Settings.DropDown;

public class DropDownSetting<T> where T : struct, Enum
{
	public virtual string Title => typeof(T).Name;

	protected virtual IReadOnlyList<T> Values { get; } = Enum.GetValues<T>();

	protected virtual string GetDisplayName(T value) => Enum.GetName(value) ?? value.ToString();

	protected virtual string? GetDescription(T value) => null;

	public IReadOnlyList<DropDownItem<T>> GetValues()
	{
		DropDownItem<T>[] result = new DropDownItem<T>[Values.Count];
		for (int i = 0; i < Values.Count; i++)
		{
			T value = Values[i];
			result[i] = new(value, GetDisplayName(value), GetDescription(value));
		}
		return result;
	}
}
