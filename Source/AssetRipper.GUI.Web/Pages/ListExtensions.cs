namespace AssetRipper.GUI.Web.Pages;

internal static class ListExtensions
{
	public static bool ContainsIndex<T>(this List<T> list, int index)
	{
		return index >= 0 && index < list.Count;
	}
}
