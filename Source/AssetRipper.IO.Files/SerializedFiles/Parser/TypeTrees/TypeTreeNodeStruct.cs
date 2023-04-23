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

		public bool IsArray
		{
			get
			{
				if (TypeName is "Array" && SubNodes.Count == 2)
				{
					TypeTreeNodeStruct sizeNode = SubNodes[0];
					return sizeNode.Name == "size" && sizeNode.SubNodes.Count == 0 && SubNodes[1].Name == "data";
				}
				return false;
			}
		}

		public bool IsVector
		{
			get
			{
				return TypeName is "vector" or "staticvector" or "set" && SubNodes.Count == 1 && SubNodes[0].IsArray;
			}
		}

		/// <summary>
		/// This is the same as a vector, except the <see cref="TypeName"/> of the element type node is used
		/// instead of "vector" or any of the other predefined type names for arrays and lists.
		/// </summary>
		/// <remarks>
		/// This was first noticed in a scriptable object where the field was a <see cref="List{T}"/> of a serializable class.
		/// </remarks>
		public bool IsNamedVector
		{
			get
			{
				return SubNodes.Count == 1 && SubNodes[0].IsArray && SubNodes[0].Name is "Array" && TypeName == SubNodes[0].SubNodes[1].TypeName;
			}
		}

		public bool IsPPtr
		{
			get
			{
				if (SubNodes.Count != 2)
				{
					return false;
				}

				TypeTreeNodeStruct fileIdNode = SubNodes[0];
				if (fileIdNode.Name is not "m_FileID" || fileIdNode.SubNodes.Count > 0)
				{
					return false;
				}

				TypeTreeNodeStruct pathIdNode = SubNodes[1];
				if (pathIdNode.Name is not "m_PathID" || pathIdNode.SubNodes.Count > 0)
				{
					return false;
				}

				//Note: custom MonoBehaviour fields have a '$' after the '<', eg PPtr<$GameObject>
				return TypeName.StartsWith("PPtr<", StringComparison.Ordinal) && TypeName.EndsWith('>');
			}
		}

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
