using AssetRipper.Import.Structure.Assembly.Serializable;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Import.Structure.Assembly.TypeTrees
{
	public sealed class SerializableTreeType : SerializableType
	{
		private SerializableTreeType(string? @namespace, string name, PrimitiveType type, int version, bool flowMappedInYaml) : base(@namespace, type, name)
		{
			Version = version;
			FlowMappedInYaml = flowMappedInYaml;
		}

		private SerializableTreeType(string name, PrimitiveType type, int version, bool flowMappedInYaml) : this(null, name, type, version, flowMappedInYaml)
		{
		}

		public override int Version { get; }
		public override bool FlowMappedInYaml { get; }

		public static SerializableTreeType FromRootNode(TypeTreeNodeStruct rootNode)
		{
			SerializableTreeType serializableTreeType = new SerializableTreeType(rootNode.TypeName, PrimitiveType.Complex, rootNode.Version, rootNode.FlowMappedInYaml);

			List<Field> fields = new();
			int startIndex = FindStartingIndex(rootNode);
			for (int i = startIndex; i < rootNode.SubNodes.Count; i++)
			{
				AddNode(rootNode.SubNodes[i], fields);
			}
			serializableTreeType.Fields = fields;
			return serializableTreeType;
		}

		private static void AddNode(TypeTreeNodeStruct node, List<Field> fields)
		{
			ToPrimititeType(node, out string typeName, out PrimitiveType primitiveType, out int arrayDepth, out bool alignBytes, out TypeTreeNodeStruct primitiveNode);

			SerializableTreeType serializableTreeType;
			if (primitiveType is PrimitiveType.Complex)
			{
				if (primitiveNode.IsPPtr)
				{
					serializableTreeType = new SerializableTreeType("UnityEngine", "Object", primitiveType, primitiveNode.Version, primitiveNode.FlowMappedInYaml);
				}
				else
				{
					serializableTreeType = FromComplexNode(typeName, primitiveNode);
				}
			}
			else
			{
				serializableTreeType = new SerializableTreeType(typeName, primitiveType, primitiveNode.Version, primitiveNode.FlowMappedInYaml);
			}

			fields.Add(new Field(serializableTreeType, arrayDepth, node.Name, alignBytes));
		}

		private static SerializableTreeType FromComplexNode(string name, TypeTreeNodeStruct node)
		{
			SerializableTreeType serializableTreeType = new SerializableTreeType(name, PrimitiveType.Complex, node.Version, node.FlowMappedInYaml);
			List<Field> fields = new();
			foreach (TypeTreeNodeStruct subNode in node.SubNodes)
			{
				AddNode(subNode, fields);
			}
			serializableTreeType.Fields = fields;
			return serializableTreeType;
		}

		private static int FindStartingIndex(TypeTreeNodeStruct rootNode)
		{
			int nameIndex = rootNode.SubNodes.IndexOf(node => node.Name == "m_Name");
			int editorClassIdIndex = rootNode.SubNodes.IndexOf(node => node.Name == "m_EditorClassIdentifier");
			int startIndex = Math.Max(nameIndex, editorClassIdIndex) + 1;
			return startIndex;
		}

		private static void ToPrimititeType(TypeTreeNodeStruct node, out string typeName, out PrimitiveType primitiveType, out int arrayDepth, out bool alignBytes, out TypeTreeNodeStruct primitiveNode)
		{
			alignBytes = false;
			typeName = "";
			arrayDepth = 0;
			while (true)
			{
				alignBytes |= node.AlignBytes;
				if (node.IsArray)
				{
					arrayDepth++;
					node = node.SubNodes[1];
				}
				else if (node.IsVector)
				{
					alignBytes |= node.SubNodes[0].AlignBytes;
					arrayDepth++;
					node = node.SubNodes[0].SubNodes[1];
				}
				else if (node.IsNamedVector)
				{
					//It's important that typeName is set before node is reassigned.
					SetIfEmpty(ref typeName, node.TypeName);
					alignBytes |= node.SubNodes[0].AlignBytes;
					arrayDepth++;
					node = node.SubNodes[0].SubNodes[1];
				}
				else
				{
					SetIfEmpty(ref typeName, node.TypeName);
					break;
				}
			}


			primitiveType = node.SubNodes.Count == 0
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
					"half" => PrimitiveType.Half,
					_ => PrimitiveType.Complex,
				}
				: node.TypeName switch
				{
					"string" => PrimitiveType.String,
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
	}
}
