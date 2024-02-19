namespace AssetRipper.Import.Configuration;

public class ParsableDataInstance<T> : DataInstance<T> where T : IParsable<T>, new()
{
	public ParsableDataInstance() : base(ParsableDataSerializer<T>.Instance)
	{
	}
}
