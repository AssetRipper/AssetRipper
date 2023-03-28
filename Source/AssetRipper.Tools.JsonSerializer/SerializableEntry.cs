using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser.TypeTrees;
using System.Text.Json.Nodes;

namespace AssetRipper.Tools.JsonSerializer;

public abstract class SerializableEntry
{
	public bool Align { get; set; }
	public abstract JsonNode? Read(ref EndianSpanReader reader);
	protected void MaybeAlign(ref EndianSpanReader reader)
	{
		if (Align)
		{
			reader.Align();
		}
	}
	public static SerializableEntry FromTypeTree(TypeTree typeTree)
	{
		int index = 0;
		return FromTypeTreeNodes(typeTree.Nodes, ref index);
	}
	public static SerializableEntry FromTypeTreeNodes(List<TypeTreeNode> list, ref int index)
	{
		bool align = list[index].MetaFlag.IsAlignBytes();
		SerializableEntry result;
		if (SerializablePrimitive.TryMakeFromTypeTreeNodes(list, ref index, out SerializablePrimitive? primitive))
		{
			result = primitive;
		}
		else
		{
			switch (list[index].Type)
			{
				case "map":
					{
						index++;
						ThrowIfIncorrectName(list[index], "Array");
						align |= list[index].MetaFlag.IsAlignBytes();
						result = SerializableArray.FromTypeTreeNodes(list, ref index);
						break;
					}
				case "vector" or "set" or "staticvector":
					{
						index++;
						ThrowIfIncorrectName(list[index], "Array");
						align |= list[index].MetaFlag.IsAlignBytes();
						result = SerializableArray.FromTypeTreeNodes(list, ref index);
						break;
					}
				case "Array":
					result = SerializableArray.FromTypeTreeNodes(list, ref index);
					break;
				case "pair":
					result = SerializablePair.FromTypeTreeNodes(list, ref index);
					break;
				case "TypelessData":
					{
						index++;
						ThrowIfIncorrectName(list[index], "size");
						index++;
						ThrowIfIncorrectName(list[index], "data");
						index++;
						result = new SerializableTypelessData();
						break;
					}
				default:
					result = SerializableType.FromTypeTreeNodes(list, ref index);
					break;
			}
		}
		result.Align |= align;
		return result;
	}
	protected static void ThrowIfIncorrectTypeName(TypeTreeNode node, string typeName)
	{
		if (typeName != node.Type)
		{
			throw new ArgumentException("Incorrect type", nameof(node));
		}
	}
	protected static void ThrowIfIncorrectName(TypeTreeNode node, string name)
	{
		if (name != node.Name)
		{
			throw new ArgumentException("Incorrect name", nameof(node));
		}
	}
}
