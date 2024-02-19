namespace AssetRipper.Import.Configuration;

public class StringDataInstance : DataInstance<string>
{
	public StringDataInstance() : base(StringDataSerializer.Instance)
	{
	}
}
