using AssetRipper.IO.Endian;

namespace AssetRipper.IO.Files.SerializedFiles.TypeTrees;

public sealed class TypeTree_1 : TypeTree<TypeTreeNode_1>
{
	public override void Read(EndianReader reader)
	{
		Nodes.Clear();
		ReadTreeNode(reader, Nodes, 0);
	}

	public override void Write(EndianWriter writer)
	{
		int index = 0;
		WriteTreeNode(writer, ref index);
	}

	private static void ReadTreeNode(EndianReader reader, List<TypeTreeNode_1> nodes, byte depth)
	{
		TypeTreeNode_1 node = new();
		node.Read(reader);
		node.Level = depth;
		nodes.Add(node);

		int childCount = reader.ReadInt32();
		for (int i = 0; i < childCount; i++)
		{
			ReadTreeNode(reader, nodes, (byte)(depth + 1));
		}
	}

	private void WriteTreeNode(EndianWriter writer, ref int index)
	{
		Nodes[index].Write(writer);
		int childCount = GetChildCount(index);
		writer.Write(childCount);
		index++;
		for (int i = 0; i < childCount; i++)
		{
			WriteTreeNode(writer, ref index);
		}
	}

	private int GetChildCount(int index)
	{
		int count = 0;
		int depth = Nodes[index].Level + 1;
		for (int i = index + 1; i < Nodes.Count; i++)
		{
			int nodeDepth = Nodes[i].Level;
			if (nodeDepth < depth)
			{
				break;
			}
			if (nodeDepth == depth)
			{
				count++;
			}
		}
		return count;
	}
}
