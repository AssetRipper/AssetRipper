using AssetRipper.IO.Endian;
using System.Text.Json.Nodes;

namespace AssetRipper.Tools.JsonSerializer;

public sealed class SerializableTypelessData : SerializableEntry
{
	public override JsonNode? Read(ref EndianSpanReader reader)
	{
		int size = reader.ReadInt32();
		byte[] data = reader.ReadBytes(size);
		if (data.Length != size)
		{
			throw new EndOfStreamException();
		}
		MaybeAlign(ref reader);
		return JsonValue.Create(Convert.ToBase64String(data));
	}
}
