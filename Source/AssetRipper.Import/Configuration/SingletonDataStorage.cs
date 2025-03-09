namespace AssetRipper.Import.Configuration;

public sealed class SingletonDataStorage : DataStorage<DataInstance>
{
	public void Add(string key, string value)
	{
		Add(key, new StringDataInstance() { Value = value });
	}

	public bool TryGetStoredValue<T>(string key, [MaybeNullWhen(false)] out T value)
	{
		if (data.TryGetValue(key, out DataInstance? storedValue) && storedValue is DataInstance<T> instance)
		{
			value = instance.Value;
			return true;
		}
		else
		{
			value = default;
			return false;
		}
	}

	public T GetStoredValue<T>(string key)
	{
		if (data.TryGetValue(key, out DataInstance? storedValue) && storedValue is DataInstance<T> instance)
		{
			return instance.Value;
		}
		else
		{
			throw new KeyNotFoundException();
		}
	}

	public void SetStoredValue<T>(string key, T value)
	{
		if (data.TryGetValue(key, out DataInstance? storedValue) && storedValue is DataInstance<T> instance)
		{
			instance.Value = value;
		}
		else
		{
			throw new KeyNotFoundException();
		}
	}
}
