using AssetRipper.IO.Endian;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.IO.Files.SerializedFiles.TypeTrees;

public partial class TypeTree<T>
{
	protected void ReadVersion10Plus(EndianReader reader)
	{
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
			T node = new();
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

	protected void WriteVersion10Plus(EndianWriter writer)
	{
		writer.Write(Nodes.Count);
		writer.Write(StringBuffer.Length);
		foreach (T node in Nodes)
		{
			node.Write(writer);
		}
		writer.Write(StringBuffer, 0, StringBuffer.Length);
	}

	private void SetNamesFromBuffer()
	{
		if (StringBuffer.Length > 0)
		{
			Dictionary<uint, string> customTypes = new();
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

			foreach (T node in Nodes)
			{
				node.Type = GetTypeName(customTypes, node.TypeStrOffset);
				node.Name = GetTypeName(customTypes, node.NameStrOffset);
			}
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
}
