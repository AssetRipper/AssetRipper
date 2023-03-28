using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles.Parser.TypeTrees;
using System.Text.Json.Nodes;

namespace AssetRipper.Tools.JsonSerializer;

public sealed class SerializableArray : SerializableEntry
{
	public SerializableArray(SerializableEntry elementType)
	{
		ElementType = elementType;
	}

	public SerializableEntry ElementType { get; }

	public override JsonNode Read(ref EndianSpanReader reader)
	{
		JsonArray result = new();
		int size = reader.ReadInt32();
		for (int i = 0; i < size; i++)
		{
			result.Add(ElementType.Read(ref reader));
		}
		MaybeAlign(ref reader);
		return result;
	}

	public static new SerializableArray FromTypeTreeNodes(List<TypeTreeNode> list, ref int index)
	{
		index++;
		ThrowIfIncorrectName(list[index], "size");
		index++;
		ThrowIfIncorrectName(list[index], "data");
		SerializableEntry elementType = SerializableEntry.FromTypeTreeNodes(list, ref index);
		return new SerializableArray(elementType);
	}
}
