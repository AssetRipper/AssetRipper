using System.Collections.Generic;
using System.Diagnostics;

namespace AssetRipper.IO.Files.SerializedFiles.Parser.TypeTrees
{
	public readonly struct TypeTreeNodeStruct
	{
		public string TypeName { get; }
		public string Name { get; }
		public int Version { get; }
		public TransferMetaFlags MetaFlag { get; }
		public IReadOnlyList<TypeTreeNodeStruct> SubNodes => subNodes;
		public bool AlignBytes => MetaFlag.IsAlignBytes();
		public bool TreatIntegerAsChar => MetaFlag.IsCharPropertyMask();

		private readonly TypeTreeNodeStruct[] subNodes;

		private TypeTreeNodeStruct(TypeTreeNode node, TypeTreeNodeStruct[] subNodes)
		{
			TypeName = node.Type;
			Name = node.Name;
			Version = node.Version;
			MetaFlag = node.MetaFlag;
			this.subNodes = subNodes;
		}

		public override string ToString() => $"{TypeName} {Name} ({subNodes?.Length ?? 0} SubNodes)";

		public static bool TryMakeFromTypeTree(TypeTree tree, out TypeTreeNodeStruct rootNode)
		{
			if (tree.Nodes.Count== 0)
			{
				rootNode = default;
				return false;
			}
			else
			{
				rootNode = FromNodeList(tree.Nodes, 0);
				return true;
			}
		}

		private static TypeTreeNodeStruct FromNodeList(List<TypeTreeNode> list, int index)
		{
			TypeTreeNode node = list[index];
			int level = node.Level;
			if (index + 1 == list.Count || list[index + 1].Level <= level)
			{
				//Has no sub nodes
				return new TypeTreeNodeStruct(node, Array.Empty<TypeTreeNodeStruct>());
			}
			else
			{
				Debug.Assert(list[index + 1].Level == level + 1);
				int subNodeCount = 0;
				for (int i = index + 1; i < list.Count; i++)
				{
					int subNodeLevel = list[i].Level;
					if (subNodeLevel == level + 1)
					{
						subNodeCount++;
					}
					else if (subNodeLevel <= level)
					{
						break;
					}
				}
				TypeTreeNodeStruct[] subNodes = new TypeTreeNodeStruct[subNodeCount];
				int subNodeIndex = 0;
				for (int i = index + 1; i < list.Count; i++)
				{
					int subNodeLevel = list[i].Level;
					if (subNodeLevel == level + 1)
					{
						subNodes[subNodeIndex] = FromNodeList(list, i);
						subNodeIndex++;
					}
					else if (subNodeLevel <= level)
					{
						break;
					}
				}
				Debug.Assert(subNodeIndex == subNodeCount);
				return new TypeTreeNodeStruct(node, subNodes);
			}
		}
	}
}
