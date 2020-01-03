using System.Collections.Generic;
using System.Text;

namespace uTinyRipper.SerializedFiles
{
	public sealed class TypeTree : ISerializedReadable, ISerializedWritable
	{
		/// <summary>
		/// 2019.3 and greater
		/// </summary>
		public static bool HasUnknown(FileGeneration generation) => generation >= FileGeneration.FG_20193_x;

		public void Read(SerializedReader reader)
		{
			if (TypeTreeNode.IsFormat5(reader.Generation))
			{
				int nodesCount = reader.ReadInt32();
				int customBufferSize = reader.ReadInt32();
				Nodes = new TypeTreeNode[nodesCount];
				for (int i = 0; i < nodesCount; i++)
				{
					TypeTreeNode node = new TypeTreeNode();
					node.Read(reader);
					Nodes[i] = node;
				}
				CustomTypeBuffer = new byte[customBufferSize];
				reader.Read(CustomTypeBuffer, 0, CustomTypeBuffer.Length);

				if (HasUnknown(reader.Generation))
				{
					Unknown = reader.ReadInt32();
				}
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
				writer.Write(CustomTypeBuffer.Length);
				for (int i = 0; i < Nodes.Length; i++)
				{
					Nodes[i].Write(writer);
				}
				writer.Write(CustomTypeBuffer, 0, CustomTypeBuffer.Length);

				if (HasUnknown(writer.Generation))
				{
					writer.Write(Unknown);
				}
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
			node.Depth = depth;
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
			int depth = Nodes[index].Depth + 1;
			for (int i = index + 1; i < Nodes.Length; i++)
			{
				int nodeDepth = Nodes[i].Depth;
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
		public byte[] CustomTypeBuffer { get; set; }
		public int Unknown { get; set; }
	}
}
