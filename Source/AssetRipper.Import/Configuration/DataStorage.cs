namespace AssetRipper.Import.Configuration;

public class DataStorage<T> where T : DataEntry
{
	protected readonly Dictionary<string, T> data = new();

	public IEnumerable<string> Keys => data.Keys;

	public T? this[string key]
	{
		get => data.TryGetValue(key, out T? value) ? value : default;
	}

	public bool TryGetValue<TValue>(string key, [NotNullWhen(true)] out TValue? value) where TValue : T
	{
		if (data.TryGetValue(key, out T? storedValue))
		{
			value = storedValue as TValue;
			return value is not null;
		}
		else
		{
			value = default;
			return false;
		}
	}

	public TValue GetValue<TValue>(string key) where TValue : T
	{
		if (TryGetValue(key, out TValue? storedValue))
		{
			return storedValue;
		}
		else
		{
			throw new KeyNotFoundException();
		}
	}

	public void Add(string key, T value)
	{
		data.Add(key, value);
	}

	public void Clear()
	{
		foreach (T value in data.Values)
		{
			value.Clear();
		}
	}
}
