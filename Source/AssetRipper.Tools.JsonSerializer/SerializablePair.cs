using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles.Parser.TypeTrees;
using System.Text.Json.Nodes;

namespace AssetRipper.Tools.JsonSerializer;

public sealed class SerializablePair : SerializableEntry
{
	public SerializableEntry First { get; }
	public SerializableEntry Second { get; }

	public SerializablePair(SerializableEntry first, SerializableEntry second)
	{
		First = first;
		Second = second;
	}

	public override JsonNode Read(ref EndianSpanReader reader)
	{
		JsonObject result = new()
		{
			{ "first", First.Read(ref reader) },
			{ "second", Second.Read(ref reader) }
		};
		MaybeAlign(ref reader);
		return result;
	}

	public static new SerializablePair FromTypeTreeNodes(List<TypeTreeNode> list, ref int index)
	{
		index++;
		ThrowIfIncorrectName(list[index], "first");
		SerializableEntry first = SerializableEntry.FromTypeTreeNodes(list, ref index);
		ThrowIfIncorrectName(list[index], "second");
		SerializableEntry second = SerializableEntry.FromTypeTreeNodes(list, ref index);
		return new SerializablePair(first, second);
	}
}
