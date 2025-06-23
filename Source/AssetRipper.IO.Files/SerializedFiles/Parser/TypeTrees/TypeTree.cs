using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles.IO;
using System.Diagnostics;
using System.Text;

namespace AssetRipper.IO.Files.SerializedFiles.Parser.TypeTrees;

public sealed class TypeTree : ISerializedReadable, ISerializedWritable
{
	public void Read(SerializedReader reader)
	{
		if (TypeTreeNode.IsFormat5(reader.Generation))
		{
			IsFormat5 = true;

			int nodesCount = reader.ReadInt32();
			if (nodesCount < 0)
			{
				throw new InvalidDataException($"Node count cannot be negative: {nodesCount}");
			}

			int stringBufferSize = reader.ReadInt32();
			if (stringBufferSize < 0)
			{
				throw new InvalidDataException($"String buffer size cannot be negative: {stringBufferSize}");
			}

			Nodes.Clear();
			Nodes.Capacity = nodesCount;
			for (int i = 0; i < nodesCount; i++)
			{
				TypeTreeNode node = new TypeTreeNode();
				node.Read(reader);
				Nodes.Add(node);
			}
			if (stringBufferSize == 0)
			{
				StringBuffer = Array.Empty<byte>();
			}
			else
			{
				StringBuffer = new byte[stringBufferSize];
				reader.Read(StringBuffer, 0, StringBuffer.Length);
			}
			SetNamesFromBuffer();
		}
		else
		{
			IsFormat5 = false;
			Nodes.Clear();
			ReadTreeNode(reader, Nodes, 0);
		}
	}

	public void Write(SerializedWriter writer)
	{
		if (TypeTreeNode.IsFormat5(writer.Generation))
		{
			writer.Write(Nodes.Count);
			writer.Write(StringBuffer.Length);
			foreach (TypeTreeNode node in Nodes)
			{
				node.Write(writer);
			}
			writer.Write(StringBuffer, 0, StringBuffer.Length);
		}
		else
		{
			int index = 0;
			WriteTreeNode(writer, ref index);
		}
	}

	private static void ReadTreeNode(SerializedReader reader, ICollection<TypeTreeNode> nodes, byte depth)
	{
		TypeTreeNode node = new TypeTreeNode();
		node.Read(reader);
		node.Level = depth;
		nodes.Add(node);

		int childCount = reader.ReadInt32();
		for (int i = 0; i < childCount; i++)
		{
			ReadTreeNode(reader, nodes, (byte)(depth + 1));
		}
	}

	private void WriteTreeNode(SerializedWriter writer, ref int index)
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

	public override string? ToString()
	{
		if (Nodes == null)
		{
			return base.ToString();
		}

		return Nodes[0].ToString();
	}

	public StringBuilder ToString(StringBuilder sb)
	{
		if (Nodes != null)
		{
			foreach (TypeTreeNode node in Nodes)
			{
				node.ToString(sb).AppendLine();
			}
		}
		return sb;
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

	public string Dump
	{
		get
		{
			StringBuilder sb = new StringBuilder();
			ToString(sb);
			return sb.ToString();
		}
	}

	private void SetNamesFromBuffer()
	{
		Debug.Assert(IsFormat5);
		Dictionary<uint, string> customTypes = new Dictionary<uint, string>();
		using (MemoryStream stream = new MemoryStream(StringBuffer))
		{
			using EndianReader reader = new EndianReader(stream, EndianType.LittleEndian);
			while (stream.Position < stream.Length)
			{
				uint position = (uint)stream.Position;
				string name = reader.ReadStringZeroTerm();
				customTypes.Add(position, name);
			}
		}

		foreach (TypeTreeNode node in Nodes)
		{
			node.Type = GetTypeName(customTypes, node.TypeStrOffset);
			node.Name = GetTypeName(customTypes, node.NameStrOffset);
		}
	}

	private static string GetTypeName(Dictionary<uint, string> customTypes, uint value)
	{
		bool isCustomType = (value & 0x80000000) == 0;
		if (isCustomType)
		{
			return customTypes[value];
		}
		else
		{
			uint offset = value & ~0x80000000;
			if (CommonString.StringBuffer.TryGetValue(offset, out string? nodeTypeName))
			{
				return nodeTypeName;
			}
			else
			{
				throw new Exception($"Unsupported asset class type name '{offset}''");
			}
		}
	}

	public List<TypeTreeNode> Nodes { get; } = new();
	public byte[] StringBuffer { get; set; } = Array.Empty<byte>();
	/// <summary>
	/// 5.0.0a1 and greater<br/>
	/// Generation 10
	/// </summary>
	private bool IsFormat5 { get; set; }
}
