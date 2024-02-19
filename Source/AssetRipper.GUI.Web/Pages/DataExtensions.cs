using AssetRipper.Import.Configuration;

namespace AssetRipper.GUI.Web.Pages;

internal static class DataExtensions
{
	public static bool ContainsIndex(this DataSet list, int index)
	{
		return index >= 0 && index < list.Count;
	}
	public static DataInstance GetOrAdd(this SingletonDataStorage storage, string key)
	{
		if (!storage.TryGetValue(key, out DataInstance? value))
		{
			value = new StringDataInstance();
			storage.Add(key, value);
		}
		return value;
	}
	public static DataSet GetOrAdd(this ListDataStorage storage, string key)
	{
		if (!storage.TryGetValue(key, out DataSet? value))
		{
			value = new StringDataSet();
			storage.Add(key, value);
		}
		return value;
	}
}
