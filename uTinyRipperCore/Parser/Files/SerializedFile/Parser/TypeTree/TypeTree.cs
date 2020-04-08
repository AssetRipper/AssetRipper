using System.Collections.Generic;
using System.Text;

namespace uTinyRipper.SerializedFiles
{
	public sealed class TypeTree : ISerializedReadable, ISerializedWritable
	{
		public void Read(SerializedReader reader)
		{
			if (TypeTreeNode.IsFormat5(reader.Generation))
			{
				int nodesCount = reader.ReadInt32();
				int stringBufferSize = reader.ReadInt32();
				Nodes = new TypeTreeNode[nodesCount];
				for (int i = 0; i < nodesCount; i++)
				{
					TypeTreeNode node = new TypeTreeNode();
					node.Read(reader);
					Nodes[i] = node;
				}
				StringBuffer = new byte[stringBufferSize];
				reader.Read(StringBuffer, 0, StringBuffer.Length);
			}
			else
			{
				List<TypeTreeNode> nodes = new List<TypeTreeNode>();
				ReadTreeNode(reader, nodes, 0);
				Nodes = nodes.ToArray();
			}
		}

		public void Write(SerializedWriter writer)
		{
			if (TypeTreeNode.IsFormat5(writer.Generation))
			{
				writer.Write(Nodes.Length);
				writer.Write(StringBuffer.Length);
				for (int i = 0; i < Nodes.Length; i++)
				{
					Nodes[i].Write(writer);
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
			for (int i = index + 1; i < Nodes.Length; i++)
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

#if DEBUG
		public string Dump
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				ToString(sb);
				return sb.ToString();
			}
		}
#endif

		public TypeTreeNode[] Nodes { get; set; }
		public byte[] StringBuffer { get; set; }
	}
}
