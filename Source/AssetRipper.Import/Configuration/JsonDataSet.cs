using System.Text.Json.Serialization.Metadata;

namespace AssetRipper.Import.Configuration;

public sealed class JsonDataSet<T> : DataSet<T> where T : new()
{
	public JsonDataSet(JsonTypeInfo<T> typeInfo) : base(new JsonDataSerializer<T>(typeInfo))
	{
	}

	public JsonDataSet(JsonTypeInfo<T> typeInfo, List<T> list) : base(new JsonDataSerializer<T>(typeInfo), list)
	{
	}
}
