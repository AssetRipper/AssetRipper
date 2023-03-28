using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles.Parser.TypeTrees;
using System.Text.Json.Nodes;

namespace AssetRipper.Tools.JsonSerializer;

public sealed class SerializableType : SerializableEntry
{
	public Dictionary<string, SerializableEntry> Entries { get; } = new();

	public override JsonNode Read(ref EndianSpanReader reader)
	{
		JsonObject result = new();
		foreach ((string key, SerializableEntry value) in Entries)
		{
			result.Add(key, value.Read(ref reader));
		}
		MaybeAlign(ref reader);
		return result;
	}

	public static new SerializableType FromTypeTreeNodes(List<TypeTreeNode> list, ref int index)
	{
		SerializableType result = new();
		int depth = list[index].Level;
		index++;
		while (index < list.Count && depth < list[index].Level)
		{
			string fieldName = list[index].Name;
			result.Entries.Add(fieldName, SerializableEntry.FromTypeTreeNodes(list, ref index));
		}
		return result;
	}
}
