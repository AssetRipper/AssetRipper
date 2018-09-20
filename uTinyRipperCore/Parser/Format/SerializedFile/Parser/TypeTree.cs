using System;
using System.Collections.Generic;

namespace uTinyRipper.SerializedFiles
{
	internal sealed class TypeTree : ISerializedFileReadable
	{
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
					
				m_nodes = new TypeTreeNode[nodesCount];
				long stringPosition = reader.BaseStream.Position + nodesCount * TypeTreeNode.NodeSize;
				for (int i = 0; i < nodesCount; i++)
				{
					TypeTreeNode node = new TypeTreeNode();
					node.Read(reader, stringPosition);
					m_nodes[i] = node;
				}
				reader.BaseStream.Position += stringSize;
			}
			else
			{
				List<TypeTreeNode> nodes = new List<TypeTreeNode>();
				ReadTreeNode(reader, nodes, 0);
				m_nodes = nodes.ToArray();
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

		public IReadOnlyList<TypeTreeNode> Nodes => m_nodes;

		private TypeTreeNode[] m_nodes;
	}
}
