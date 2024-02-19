namespace AssetRipper.Import.Configuration;

public abstract class DataInstance : DataEntry
{
	public abstract string Text { get; set; }
}
public class DataInstance<T> : DataInstance
{
	private readonly DataSerializer<T> serializer;
	public DataInstance(DataSerializer<T> serializer)
	{
		this.serializer = serializer;
		Value = serializer.CreateNew();
	}
	public DataInstance(DataSerializer<T> serializer, T value)
	{
		this.serializer = serializer;
		Value = value;
	}
	public T Value { get; set; }
	public sealed override string Text
	{
		get => serializer.Serialize(Value);
		set => Value = serializer.Deserialize(value);
	}
	public sealed override void Clear() => Value = serializer.CreateNew();
}
