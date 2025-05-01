using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser.TypeTrees;
using AssetRipper.SerializationLogic;
using AssetRipper.SourceGenerated;
using AssetRipper.Tpk;
using AssetRipper.Tpk.Shared;
using AssetRipper.Tpk.TypeTrees;
using System.Collections;
using System.Diagnostics;

namespace AssetRipper.Import.Structure.Assembly.TypeTrees;

public readonly struct TypeTreeNodeStruct : IReadOnlyList<TypeTreeNodeStruct>, IEquatable<TypeTreeNodeStruct>
{
	[field: MaybeNull]
	private static TpkTypeTreeBlob TypeTreeBlob
	{
		get
		{
			field ??= (TpkTypeTreeBlob)TpkFile.FromStream(SourceTpk.GetStream()).GetDataBlob();
			return field;
		}
	}

	public string TypeName { get; }
	public string Name { get; }
	public int Version { get; }
	public TransferMetaFlags MetaFlag { get; }
	public IReadOnlyList<TypeTreeNodeStruct> SubNodes => subNodes;
	public bool AlignBytes => MetaFlag.IsAlignBytes();
	public bool TreatIntegerAsChar => MetaFlag.IsCharPropertyMask();
	public bool FlowMappedInYaml => MetaFlag.IsTransferUsingFlowMappingStyle();

	private readonly TypeTreeNodeStruct[] subNodes;

	public int Count => subNodes.Length;

	public TypeTreeNodeStruct this[int index] => subNodes[index];

	public TypeTreeNodeStruct this[string name] => subNodes.First(t => t.Name == name);

	public bool IsArray
	{
		get
		{
			if (TypeName is "Array" or "TypelessData" && SubNodes.Count == 2)
			{
				TypeTreeNodeStruct sizeNode = SubNodes[0];
				return sizeNode.Name == "size" && sizeNode.SubNodes.Count == 0 && SubNodes[1].Name == "data";
			}
			return false;
		}
	}

	public bool IsTypelessData
	{
		get
		{
			if (TypeName is "TypelessData" && SubNodes.Count == 2)
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
			if (SubNodes.Count == 1 && SubNodes[0].IsArray && SubNodes[0].Name is "Array")
			{
				string elementTypeName = SubNodes[0].SubNodes[1].TypeName;
				return elementTypeName == TypeName || elementTypeName is "Generic Mono";
				//Generic Mono has only been found on Unity 3.
				//https://github.com/AssetRipper/AssetRipper/issues/1328
			}
			return false;
		}
	}

	public bool IsPair
	{
		get
		{
			return TypeName is "pair" && SubNodes.Count == 2 && SubNodes[0].Name == "first" && SubNodes[1].Name == "second";
		}
	}

	public bool IsMap
	{
		get
		{
			return TypeName is "map" && SubNodes.Count == 1 && SubNodes[0].IsArray && SubNodes[0].SubNodes[1].IsPair;
		}
	}

	/// <summary>
	/// This contains the data for an asset's [SerializeReference] fields.
	/// </summary>
	/// <remarks>
	/// This is the last top-level field in the asset's type tree.
	/// </remarks>
	public bool IsManagedReferencesRegistry
	{
		get
		{
			return TypeName is "ManagedReferencesRegistry" && Name is "references" && SubNodes.Count > 1;
		}
	}

	/// <summary>
	/// This is the data for a [SerializeReference] object reference.
	/// </summary>
	/// <remarks>
	/// If the type tree is flattened into a list, this is the last entry in the list.
	/// </remarks>
	public bool IsReferencedObjectData
	{
		get
		{
			return TypeName is "ReferencedObjectData" && Name is "data" && SubNodes.Count is 0;
		}
	}

	public bool IsByte
	{
		get
		{
			return Count is 0 && TypeName is "char" or "UInt8";
		}
	}

	public bool IsString
	{
		get
		{
			return Count is 1 && TypeName is "string" && this[0].IsArray && this[0][1].IsByte;
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

	public TypeTreeNodeStruct(string typeName, string name, int version, TransferMetaFlags metaFlags, TypeTreeNodeStruct[] subNodes)
	{
		TypeName = typeName;
		Name = name;
		Version = version;
		MetaFlag = metaFlags;
		this.subNodes = subNodes;
	}

	private TypeTreeNodeStruct(TypeTreeNode node, TypeTreeNodeStruct[] subNodes)
	{
		TypeName = node.Type;
		Name = node.Name;
		Version = node.Version;
		MetaFlag = node.MetaFlag;
		this.subNodes = subNodes;
	}

	private TypeTreeNodeStruct(TpkUnityNode node, TypeTreeNodeStruct[] subNodes, TpkStringBuffer stringBuffer)
	{
		TypeName = stringBuffer[node.TypeName];
		Name = stringBuffer[node.Name];
		Version = node.Version;
		MetaFlag = (TransferMetaFlags)node.MetaFlag;
		this.subNodes = subNodes;
	}

	public override string ToString() => $"{TypeName} {Name} ({subNodes?.Length ?? 0} SubNodes)";

	public static bool TryMakeFromTypeTree(TypeTree tree, out TypeTreeNodeStruct rootNode)
	{
		if (tree.Nodes.Count == 0)
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

	public static bool TryMakeFromTpk(ClassIDType classID, UnityVersion version, out TypeTreeNodeStruct releaseTree, out TypeTreeNodeStruct editorTree)
	{
		TpkTypeTreeBlob blob = TypeTreeBlob;
		TpkClassInformation? classInformation = blob.ClassInformation.FirstOrDefault(c => c.ID == (int)classID);
		if (classInformation is not null)
		{
			TpkUnityClass @class = GetItemForVersion(classInformation.Classes, version);
			releaseTree = FromTpkNode(blob.NodeBuffer[@class.ReleaseRootNode], blob.StringBuffer, blob.NodeBuffer);
			editorTree = FromTpkNode(blob.NodeBuffer[@class.EditorRootNode], blob.StringBuffer, blob.NodeBuffer);
			return true;
		}

		releaseTree = default;
		editorTree = default;
		return false;
	}

	private static TypeTreeNodeStruct FromTpkNode(TpkUnityNode node, TpkStringBuffer stringBuffer, TpkUnityNodeBuffer nodeBuffer)
	{
		TypeTreeNodeStruct[] subNodes = new TypeTreeNodeStruct[node.SubNodes.Length];
		for (int i = 0; i < node.SubNodes.Length; i++)
		{
			subNodes[i] = FromTpkNode(nodeBuffer[node.SubNodes[i]], stringBuffer, nodeBuffer);
		}
		return new(node, subNodes, stringBuffer);
	}

	private static TypeTreeNodeStruct FromNodeList(List<TypeTreeNode> list, int index)
	{
		TypeTreeNode node = list[index];
		int level = node.Level;
		if (index + 1 == list.Count || list[index + 1].Level <= level)
		{
			//Has no sub nodes
			return new TypeTreeNodeStruct(node, []);
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

	public static TypeTreeNodeStruct FromSerializableType(SerializableType type)
	{
		return FromSerializableType(type, "Base", false, out _);
	}

	private static TypeTreeNodeStruct FromSerializableType(SerializableType type, string name, bool alignBytes, out bool anyChildUsesAlignBytes)
	{
		anyChildUsesAlignBytes = false;

		TypeTreeNodeStruct[] subNodes;
		switch (type.Fields.Count)
		{
			case 0:
				if (type.Type is PrimitiveType.String)
				{
					subNodes =
					[
						new("Array", "Array", 1, TransferMetaFlags.NoTransferFlags,
						[
							new("SInt32", "size", 1, TransferMetaFlags.NoTransferFlags, []),
							new("char", "data", 1, TransferMetaFlags.NoTransferFlags, []),
						]),
					];
				}
				else if (type.IsEnginePointer())
				{
					subNodes =
					[
						new("SInt32", "m_FileID", 1, TransferMetaFlags.NoTransferFlags, []),
						new("SInt64", "m_PathID", 1, TransferMetaFlags.NoTransferFlags, []),
						// Might need to handle m_PathID being SInt32 on older versions.
					];
				}
				else
				{
					subNodes = [];
				}

				break;
			default:
				subNodes = new TypeTreeNodeStruct[type.Fields.Count];

				for (int i = 0; i < type.Fields.Count; i++)
				{
					SerializableType.Field field = type.Fields[i];
					subNodes[i] = FromSerializableTypeField(field, out bool anyFieldChildUsesAlignBytes);
					anyChildUsesAlignBytes |= field.Align | anyFieldChildUsesAlignBytes;
				}
				break;
		}

		TransferMetaFlags metaFlag = TransferMetaFlags.NoTransferFlags;
		if (alignBytes)
		{
			metaFlag |= TransferMetaFlags.AlignBytes;
		}
		if (anyChildUsesAlignBytes)
		{
			metaFlag |= TransferMetaFlags.AnyChildUsesAlignBytes;
		}
		if (type.FlowMappedInYaml)
		{
			metaFlag |= TransferMetaFlags.TransferUsingFlowMappingStyle;
		}
		if (type.Type is PrimitiveType.Char)
		{
			metaFlag |= TransferMetaFlags.CharPropertyMask;
		}

		string typeName = type.Type switch
		{
			PrimitiveType.Bool => "bool",
			PrimitiveType.Char => "UInt16",
			PrimitiveType.Byte => "UInt8",
			PrimitiveType.SByte => "SInt8",
			PrimitiveType.Short => "SInt16",
			PrimitiveType.UShort => "UInt16",
			PrimitiveType.Int => "SInt32",
			PrimitiveType.UInt => "UInt32",
			PrimitiveType.Long => "SInt64",
			PrimitiveType.ULong => "UInt64",
			PrimitiveType.Single => "float",
			PrimitiveType.Double => "double",
			PrimitiveType.String => "string",
			PrimitiveType.Complex => type.IsEnginePointer() ? "PPtr<Object>" : type.Name,
			_ => type.Name,
		};

		return new(typeName, name, type.Version, metaFlag, subNodes);
	}

	private static TypeTreeNodeStruct FromSerializableTypeField(SerializableType.Field field, out bool anyChildUsesAlignBytes)
	{
		switch (field.ArrayDepth)
		{
			case 0:
				return FromSerializableType(field.Type, field.Name, field.Align, out anyChildUsesAlignBytes);
			case 1:
				{
					// Note: treatment of alignment is incorrect.
					// In particular, this code treats field.Align as shared between the array and its elements.
					// This is certainly not correct, but fixing it to store separate would require changing the code in a bunch of places.
					// Also, alignment is not important for the current use case. I just included it to lay the groundwork for future use.

					TypeTreeNodeStruct sizeNode = new("int", "size", 1, TransferMetaFlags.NoTransferFlags, []);
					TypeTreeNodeStruct dataNode = FromSerializableType(field.Type, "data", field.Align, out anyChildUsesAlignBytes);
					anyChildUsesAlignBytes |= field.Align;

					TransferMetaFlags metaFlag = TransferMetaFlags.NoTransferFlags;
					if (field.Align)
					{
						metaFlag |= TransferMetaFlags.AlignBytes;
					}
					if (anyChildUsesAlignBytes)
					{
						metaFlag |= TransferMetaFlags.AnyChildUsesAlignBytes;
					}

					return new("Array", field.Name, 0, metaFlag, [sizeNode, dataNode]);
				}
			case 2:
				throw new NotImplementedException("Array depth 2 is not implemented.");
			default:
				throw new NotSupportedException($"Array depth {field.ArrayDepth} is not supported.");
		}
	}

	public IEnumerator<TypeTreeNodeStruct> GetEnumerator() => SubNodes.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => SubNodes.GetEnumerator();

	public override bool Equals(object? obj)
	{
		return obj is TypeTreeNodeStruct @struct && Equals(@struct);
	}

	public bool Equals(TypeTreeNodeStruct other)
	{
		if (TypeName != other.TypeName || Name != other.Name || Version != other.Version || MetaFlag != other.MetaFlag)
		{
			return false;
		}

		if (ReferenceEquals(SubNodes, other.SubNodes))
		{
			return true;
		}

		if (SubNodes is null || other.SubNodes is null || SubNodes.Count != other.SubNodes.Count)
		{
			return false;
		}

		for (int i = 0; i < SubNodes.Count; i++)
		{
			if (!SubNodes[i].Equals(other.SubNodes[i]))
			{
				return false;
			}
		}

		return true;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(TypeName, Name, Version, MetaFlag, SubNodes.Count);
	}

	public static bool operator ==(TypeTreeNodeStruct left, TypeTreeNodeStruct right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(TypeTreeNodeStruct left, TypeTreeNodeStruct right)
	{
		return !(left == right);
	}

	private static TpkUnityClass GetItemForVersion(List<KeyValuePair<UnityVersion, TpkUnityClass?>> list, UnityVersion version)
	{
		if (list.Count == 0)
		{
			throw new ArgumentException("List is empty.", nameof(list));
		}

		if (list[0].Key > version)
		{
			return list[0].Value ?? throw new ArgumentException("First element should never be null.", nameof(list));
		}

		for (int i = 0; i < list.Count - 1; i++)
		{
			if (list[i].Key <= version && version < list[i + 1].Key)
			{
				TpkUnityClass? result = list[i].Value;
				if (result is not null)
				{
					return result;
				}
				else
				{
					return list[i - 1].Value ?? throw new ArgumentException("Consecutive elements should never both be null.", nameof(list));
				}
			}
		}

		if (list[^1].Value is null)
		{
			return list[^2].Value ?? throw new ArgumentException("Second to last element should never be null when the last element is null.", nameof(list));
		}
		else
		{
			return list[^1].Value!;
		}
	}
}
