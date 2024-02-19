namespace AssetRipper.Import.Configuration;

public sealed class StringDataSet : DataSet<string>
{
	public StringDataSet() : base(StringDataSerializer.Instance)
	{
	}

	public StringDataSet(List<string> list) : base(StringDataSerializer.Instance, list)
	{
	}
}
