namespace AssetRipper.Import.Configuration;

public abstract class DataSerializer<T>
{
	public abstract T Deserialize(string text);
	public abstract string Serialize(T value);
	public abstract T CreateNew();
}
