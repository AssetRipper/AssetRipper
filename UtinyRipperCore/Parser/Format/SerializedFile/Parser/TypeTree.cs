using System;
using System.Collections.Generic;

namespace UtinyRipper.SerializedFiles
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

		public void Read(SerializedFileStream stream)
		{
			if (IsRead5Format(stream.Generation))
			{
				int nodesCount = stream.ReadInt32();
				if(nodesCount < 0)
				{
					throw new Exception($"Invalid type tree's node count {nodesCount}");
				}

				int stringSize = stream.ReadInt32();
				if (stringSize < 0)
				{
					throw new Exception($"Invalid type tree's string size {stringSize}");
				}
					
				m_nodes = new TypeTreeNode[nodesCount];
				long stringPosition = stream.BaseStream.Position + nodesCount * TypeTreeNode.NodeSize;
				for (int i = 0; i < nodesCount; i++)
				{
					TypeTreeNode node = new TypeTreeNode();
					node.Read(stream, stringPosition);
					m_nodes[i] = node;
				}
				stream.BaseStream.Position += stringSize;
			}
			else
			{
				List<TypeTreeNode> nodes = new List<TypeTreeNode>();
				ReadTreeNode(stream, nodes, 0);
				m_nodes = nodes.ToArray();
			}
		}

		private static void ReadTreeNode(SerializedFileStream stream, ICollection<TypeTreeNode> nodes, byte depth)
		{
			TypeTreeNode node = new TypeTreeNode(depth);
			node.Read(stream);
			nodes.Add(node);

			int childCount = stream.ReadInt32();
			for (int i = 0; i < childCount; i++)
			{
				ReadTreeNode(stream, nodes, (byte)(depth + 1));
			}
		}

		public IReadOnlyList<TypeTreeNode> Nodes => m_nodes;

		private TypeTreeNode[] m_nodes;
	}
}
