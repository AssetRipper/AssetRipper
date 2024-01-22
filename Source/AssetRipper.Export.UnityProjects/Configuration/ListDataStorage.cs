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
}
