namespace AssetRipper.Export.UnityProjects.Configuration;

public sealed class ListDataStorage : DataStorage<List<string>>
{
	public void Add(string key, string value)
	{
		if (data.TryGetValue(key, out List<string>? list) && list is not null)
		{
			list.Add(value);
		}
		else
		{
			data[key] = [value];
		}
	}

	public List<string> GetOrAdd(string key)
	{
		if (data.TryGetValue(key, out List<string>? list))
		{
			return list;
		}
		else
		{
			List<string> newList = new();
			data[key] = newList;
			return newList;
		}
	}
}
