namespace AssetRipper.Import.Configuration;

public sealed class ParsableDataSerializer<T> : DataSerializer<T> where T : IParsable<T>, new()
{
	public static ParsableDataSerializer<T> Instance { get; } = new();

	public override T Deserialize(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return CreateNew();
		}

		// Forgiving parsing
		if (T.TryParse(text, null, out T? value))
		{
			return value;
		}
		else
		{
			return CreateNew();
		}
	}

	public override string Serialize(T value) => value.ToString() ?? "";
	public override T CreateNew() => new();
}
