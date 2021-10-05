using AssetRipper.Core.Parser.Files.SerializedFiles.IO;
using System.Collections.Generic;
using System.Text;

namespace AssetRipper.Core.Parser.Files.SerializedFiles.Parser.TypeTree
{
	public sealed class TypeTree : ISerializedReadable, ISerializedWritable
	{
		public void Read(SerializedReader reader)
		{
			if (TypeTreeNode.IsFormat5(reader.Generation))
			{
				int nodesCount = reader.ReadInt32();
				int stringBufferSize = reader.ReadInt32();
				Nodes = new List<TypeTreeNode>(nodesCount);
				for (int i = 0; i < nodesCount; i++)
				{
					TypeTreeNode node = new TypeTreeNode();
					node.Read(reader);
					Nodes.Add(node);
				}
				StringBuffer = new byte[stringBufferSize];
				reader.Read(StringBuffer, 0, StringBuffer.Length);
			}
			else
			{
				ReadTreeNode(reader, Nodes, 0);
			}
		}

		public void Write(SerializedWriter writer)
		{
			if (TypeTreeNode.IsFormat5(writer.Generation))
			{
				writer.Write(Nodes.Count);
				writer.Write(StringBuffer.Length);
				foreach(var node in Nodes)
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

		public override string ToString()
		{
			if (Nodes == null)
			{
				return base.ToString();
			}

			return Nodes[0].ToString();
		}

		public StringBuilder ToString(StringBuilder sb)
		{
			foreach (TypeTreeNode node in Nodes)
			{
				node.ToString(sb).AppendLine();
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

		public void SetNamesFromBuffer()
		{
			if(StringBuffer != null && StringBuffer.Length > 0)
			{
				foreach (var node in Nodes)
				{
					node.Name = GetString(node.NameStrOffset);
					node.Type = GetString(node.TypeStrOffset);
				}
			}
		}

		private string GetString(uint offset)
		{
			string str = "";
			for (uint i = offset; i < StringBuffer.Length && StringBuffer[i] != 0; i++)
			{
				str += (char)StringBuffer[i];
			}
			return str;
		}

		public List<TypeTreeNode> Nodes { get; set; }
		public byte[] StringBuffer { get; set; }
	}
}
