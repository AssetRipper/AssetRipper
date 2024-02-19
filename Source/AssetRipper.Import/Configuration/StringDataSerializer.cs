namespace AssetRipper.Import.Configuration;

public sealed class StringDataSerializer : DataSerializer<string>
{
	public static StringDataSerializer Instance { get; } = new();

	public override string Deserialize(string text) => text;
	public override string Serialize(string value) => value;
	public override string CreateNew() => "";
}
