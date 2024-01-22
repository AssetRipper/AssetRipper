namespace AssetRipper.Export.UnityProjects.Configuration;

public class DataStorage<T> where T : class
{
	protected readonly Dictionary<string, T> data = new();

	public T? this[string key]
	{
		get => data.TryGetValue(key, out T? value) ? value : default;
		set
		{
			if (value is null)
			{
				data.Remove(key);
			}
			else
			{
				data[key] = value;
			}
		}
	}

	public bool TryGetValue(string key, [NotNullWhen(true)] out T? value)
	{
		return data.TryGetValue(key, out value);
	}

	public void Clear()
	{
		data.Clear();
	}
}
