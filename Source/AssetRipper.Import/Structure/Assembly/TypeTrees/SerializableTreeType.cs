using AssetRipper.SerializationLogic;
using AssetRipper.SourceGenerated.Extensions;
using System.Diagnostics;

namespace AssetRipper.Import.Structure.Assembly.TypeTrees
{
	public sealed class SerializableTreeType : SerializableType
	{
		private SerializableTreeType(string name, PrimitiveType type, int version, bool flowMappedInYaml) : base(null, type, name)
		{
			Version = version;
			FlowMappedInYaml = flowMappedInYaml;
		}

		public override int Version { get; }
		public override bool FlowMappedInYaml { get; }

		public static SerializableTreeType FromRootNode(TypeTreeNodeStruct rootNode, bool monoBehaviourStructure = false)
		{
			ToPrimititeType(rootNode, out string typeName, out PrimitiveType primitiveType, out int arrayDepth, out _, out TypeTreeNodeStruct primitiveNode);
			Debug.Assert(arrayDepth == 0, "Array depth should be 0 for root node");
			Debug.Assert(primitiveNode == rootNode, "Primitive node should be the same as root node");
			Debug.Assert(!monoBehaviourStructure || primitiveType is PrimitiveType.Complex, "MonoBehaviour structure should be complex type");

			SerializableTreeType serializableTreeType = new SerializableTreeType(typeName, primitiveType, rootNode.Version, rootNode.FlowMappedInYaml);

			List<Field> fields = new();
			int startIndex = monoBehaviourStructure ? FindStartingIndexForMonoBehaviour(rootNode) : 0;
			for (int i = startIndex; i < rootNode.SubNodes.Count; i++)
			{
				AddNode(rootNode.SubNodes[i], fields);
			}
			serializableTreeType.Fields = fields;
			serializableTreeType.SetMaxDepth();
			return serializableTreeType;
		}

		private static void AddNode(TypeTreeNodeStruct node, List<Field> fields)
		{
			ToPrimititeType(node, out string typeName, out PrimitiveType primitiveType, out int arrayDepth, out bool alignBytes, out TypeTreeNodeStruct primitiveNode);

			SerializableType serializableType;
			if (primitiveType is PrimitiveType.Complex or PrimitiveType.Pair or PrimitiveType.MapPair)
			{
				if (primitiveNode.IsPPtr)
				{
					serializableType = SerializablePointerType.Shared;
				}
				else
				{
					serializableType = FromStructureNode(typeName, primitiveNode, primitiveType);
				}
			}
			else
			{
				serializableType = new SerializableTreeType(typeName, primitiveType, primitiveNode.Version, primitiveNode.FlowMappedInYaml)
				{
					MaxDepth = 0
				};
			}

			fields.Add(new Field(serializableType, arrayDepth, node.Name, alignBytes));
		}

		private static SerializableTreeType FromStructureNode(string name, TypeTreeNodeStruct node, PrimitiveType type)
		{
			SerializableTreeType serializableTreeType = new SerializableTreeType(name, type, node.Version, node.FlowMappedInYaml);
			List<Field> fields = new();
			foreach (TypeTreeNodeStruct subNode in node.SubNodes)
			{
				AddNode(subNode, fields);
			}
			serializableTreeType.Fields = fields;
			serializableTreeType.SetMaxDepth();
			return serializableTreeType;
		}

		private static int FindStartingIndexForMonoBehaviour(TypeTreeNodeStruct rootNode)
		{
			int nameIndex = rootNode.SubNodes.IndexOf(node => node.Name == "m_Name");
			int editorClassIdIndex = rootNode.SubNodes.IndexOf(node => node.Name == "m_EditorClassIdentifier");
			int startIndex = Math.Max(nameIndex, editorClassIdIndex) + 1;
			return startIndex;
		}

		private static void ToPrimititeType(TypeTreeNodeStruct node, out string typeName, out PrimitiveType primitiveType, out int arrayDepth, out bool alignBytes, out TypeTreeNodeStruct primitiveNode)
		{
			bool isMap = false;
			alignBytes = false;
			typeName = "";
			arrayDepth = 0;
			while (true)
			{
				alignBytes |= node.AlignBytes;
				if (node.IsArray)
				{
					arrayDepth++;
					node = node[1];
				}
				else if (node.IsVector)
				{
					alignBytes |= node[0].AlignBytes;
					arrayDepth++;
					node = node[0][1];
				}
				else if (node.IsNamedVector)
				{
					//It's important that typeName is set before node is reassigned.
					SetIfEmpty(ref typeName, node.TypeName);
					alignBytes |= node[0].AlignBytes;
					arrayDepth++;
					node = node[0][1];
				}
				else if (node.IsMap)
				{
					isMap = true;
					alignBytes |= node[0].AlignBytes;
					arrayDepth++;
					node = node[0][1];
				}
				else
				{
					SetIfEmpty(ref typeName, node.TypeName);
					break;
				}
			}


			primitiveType = node.Count == 0
				? node.TypeName switch
				{
					"bool" => PrimitiveType.Bool,
					"char" or "UInt8" => PrimitiveType.Byte,
					"SInt8" => PrimitiveType.SByte,
					"short" or "SInt16" => PrimitiveType.Short,
					"ushort" or "UInt16" or "unsigned short" => node.TreatIntegerAsChar ? PrimitiveType.Char : PrimitiveType.UShort,
					"int" or "SInt32" or "Type*" => PrimitiveType.Int,
					"uint" or "UInt32" or "unsigned int" => PrimitiveType.UInt,
					"SInt64" or "long long" => PrimitiveType.Long,
					"UInt64" or "FileSize" or "unsigned long long" => PrimitiveType.ULong,
					"float" => PrimitiveType.Single,
					"double" => PrimitiveType.Double,
					_ => PrimitiveType.Complex,
				}
				: false switch
				{
					_ when node.IsString => PrimitiveType.String,
					_ when node.IsPair => isMap ? PrimitiveType.MapPair : PrimitiveType.Pair,
					_ => PrimitiveType.Complex,
				};
			primitiveNode = node;

			static void SetIfEmpty(ref string local, string value)
			{
				if (string.IsNullOrEmpty(local))
				{
					local = value;
				}
			}
		}

		private void SetMaxDepth()
		{
			int maxDepth = 0;
			foreach (Field field in Fields)
			{
				Debug.Assert(field.Type.IsMaxDepthKnown, "The depth of this type is not known.");
				maxDepth = Math.Max(maxDepth, field.Type.MaxDepth + 1);
			}
			MaxDepth = maxDepth;
		}
	}
}
