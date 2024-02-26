using AssetRipper.Import.Structure.Assembly.Serializable;
using AssetRipper.IO.Files.SerializedFiles.Parser.TypeTrees;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Import.Structure.Assembly.TypeTrees
{
	public sealed class SerializableTreeType : SerializableType
	{
		private SerializableTreeType(string? @namespace, string name, PrimitiveType type, int version) : base(@namespace, type, name)
		{
			Version = version;
		}

		private SerializableTreeType(string name, PrimitiveType type, int version) : this(null, name, type, version)
		{
		}

		public override int Version { get; }

		public static SerializableTreeType FromRootNode(TypeTreeNodeStruct rootNode)
		{
			SerializableTreeType serializableTreeType = new SerializableTreeType(rootNode.TypeName, PrimitiveType.Complex, rootNode.Version);

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
			ToPrimititeType(node, out PrimitiveType primitiveType, out int arrayDepth, out TypeTreeNodeStruct primitiveNode);

			SerializableTreeType serializableTreeType;
			if (primitiveType is PrimitiveType.Complex)
			{
				if (primitiveNode.IsPPtr)
				{
					serializableTreeType = new SerializableTreeType("UnityEngine", "Object", primitiveType, primitiveNode.Version);
				}
				else
				{
					serializableTreeType = FromComplexNode(node.TypeName, primitiveNode);
				}
			}
			else
			{
				serializableTreeType = new SerializableTreeType(node.TypeName, primitiveType, primitiveNode.Version);
			}

			fields.Add(new Field(serializableTreeType, arrayDepth, node.Name));
		}

		private static SerializableTreeType FromComplexNode(string name, TypeTreeNodeStruct node)
		{
			SerializableTreeType serializableTreeType = new SerializableTreeType(name, PrimitiveType.Complex, node.Version);
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

		private static void ToPrimititeType(TypeTreeNodeStruct node, out PrimitiveType primitiveType, out int arrayDepth, out TypeTreeNodeStruct primitiveNode)
		{
			arrayDepth = 0;
			while (true)
			{
				if (node.IsArray)
				{
					arrayDepth++;
					node = node.SubNodes[1];
				}
				else if (node.IsVector || node.IsNamedVector)
				{
					arrayDepth++;
					node = node.SubNodes[0].SubNodes[1];
				}
				else
				{
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
		}
	}
}
