namespace AssetRipper.Import.Configuration;

public sealed class ListDataStorage : DataStorage<DataSet>
{
	public void Add(string key, List<string> value)
	{
		Add(key, new StringDataSet(value));
	}

	public void Add<T>(string key, List<T> value) where T : IParsable<T>, new()
	{
		Add(key, new ParsableDataSet<T>(value));
	}
}
