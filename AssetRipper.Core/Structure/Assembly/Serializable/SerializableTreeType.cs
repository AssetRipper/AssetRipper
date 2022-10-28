using AssetRipper.IO.Files.SerializedFiles.Parser.TypeTrees;
using AssetRipper.SourceGenerated.Enums;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AssetRipper.Core.Structure.Assembly;
using Microsoft.VisualBasic.FileIO;

namespace AssetRipper.Core.Structure.Assembly.Serializable
{
	public class SerializableTreeType : SerializableType
	{
		public SerializableTreeType(string name, PrimitiveType type) : base("", type, name)
		{
		}


		private void ToPrimititeType(TypeTreeNodeStruct node, out PrimitiveType primitiveType, out int arrayDepth, out TypeTreeNodeStruct primitiveNode)
		{
			arrayDepth = 0;
			while (true)
			{
				if (node.TypeName is "Array")
				{
					arrayDepth++;
					node = node.SubNodes[1];
				}
				else if (node.TypeName is "vector" or "staticvector" or "set")
				{
					node = node.SubNodes[0];
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

		private void AddNode(TypeTreeNodeStruct node, SerializableTreeType treeType)
		{
			ToPrimititeType(node, out PrimitiveType primitiveType, out int arrayDepth, out TypeTreeNodeStruct primitiveNode);

			SerializableTreeType serializableTreeType = new(node.Name, primitiveType);
			if (primitiveType == PrimitiveType.Complex)
			{
				foreach (TypeTreeNodeStruct subNode in primitiveNode.SubNodes)
				{
					AddNode(subNode, serializableTreeType);
				}
			}

			treeType.Fields = treeType.Fields.Append(new Field(
				serializableTreeType,
				arrayDepth,
				node.Name)).ToList();
		}

		public SerializableTreeType FromTypeTree(TypeTree tree)
		{
			string[] skipObjects = { "m_ObjectHideFlags", "m_CorrespondingSourceObject", "m_PrefabInstance", "m_PrefabAsset", "m_GameObject", "m_Enabled", "m_EditorHideFlags", "m_Script", "m_Name", "m_EditorClassIdentifier" };

			TypeTreeNodeStruct.TryMakeFromTypeTree(tree, out TypeTreeNodeStruct rootNode);

			foreach (TypeTreeNodeStruct subNode in rootNode.SubNodes)
			{
				if (!skipObjects.Contains(subNode.Name))
					AddNode(subNode, this);
			}

			return this;
		}
	}
}
