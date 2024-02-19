namespace AssetRipper.Import.Configuration;

public sealed class ParsableDataSet<T> : DataSet<T> where T : IParsable<T>, new()
{
	public ParsableDataSet() : base(ParsableDataSerializer<T>.Instance)
	{
	}

	public ParsableDataSet(List<T> list) : base(ParsableDataSerializer<T>.Instance, list)
	{
	}
}
