using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uTinyRipper.SerializedFiles
{
	internal sealed class TypeTree : ISerializedFileReadable
	{
		public TypeTree()
		{
		}

		public TypeTree(IReadOnlyCollection<TypeTreeNode> nodes)
		{
			Nodes = nodes.ToArray();
		}

		/// <summary>
		/// 5.0.0a1 and greater
		/// </summary>
		private static bool IsRead5Format(FileGeneration generation)
		{
			return generation == FileGeneration.FG_500a1 || generation >= FileGeneration.FG_500aunk1;
		}

		public void Read(SerializedFileReader reader)
		{
			if (IsRead5Format(reader.Generation))
			{
				int nodesCount = reader.ReadInt32();
				if(nodesCount < 0)
				{
					throw new Exception($"Invalid type tree's node count {nodesCount}");
				}

				int stringSize = reader.ReadInt32();
				if (stringSize < 0)
				{
					throw new Exception($"Invalid type tree's string size {stringSize}");
				}

				TypeTreeNode[] nodes = new TypeTreeNode[nodesCount];
				long stringPosition = reader.BaseStream.Position + nodesCount * TypeTreeNode.GetNodeSize(reader.Generation);
				for (int i = 0; i < nodesCount; i++)
				{
					TypeTreeNode node = new TypeTreeNode();
					node.Read(reader, stringPosition);
					nodes[i] = node;
				}
				Nodes = nodes;
				reader.BaseStream.Position += stringSize;
			}
			else
			{
				List<TypeTreeNode> nodes = new List<TypeTreeNode>();
				ReadTreeNode(reader, nodes, 0);
				Nodes = nodes.ToArray();
			}
		}

		private static void ReadTreeNode(SerializedFileReader reader, ICollection<TypeTreeNode> nodes, byte depth)
		{
			TypeTreeNode node = new TypeTreeNode(depth);
			node.Read(reader);
			nodes.Add(node);

			int childCount = reader.ReadInt32();
			for (int i = 0; i < childCount; i++)
			{
				ReadTreeNode(reader, nodes, (byte)(depth + 1));
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

		public IReadOnlyList<TypeTreeNode> Nodes { get; private set; }
	}
}
