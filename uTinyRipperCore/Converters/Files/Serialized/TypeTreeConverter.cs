using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Converters
{
	public static class TypeTreeConverter
	{
		public static void CombineFormats(FormatVersion generation, TypeTree origin)
		{
			if (TypeTreeNode.IsFormat5(generation))
			{
				Dictionary<uint, string> customTypes = new Dictionary<uint, string>();
				using (MemoryStream stream = new MemoryStream(origin.StringBuffer))
				{
					using (EndianReader reader = new EndianReader(stream, EndianType.LittleEndian))
					{
						while (stream.Position < stream.Length)
						{
							uint position = (uint)stream.Position;
							string name = reader.ReadStringZeroTerm();
							customTypes.Add(position, name);
						}
					}
				}

				foreach (TypeTreeNode node in origin.Nodes)
				{
					node.Type = GetTypeName(customTypes, node.TypeStrOffset);
					node.Name = GetTypeName(customTypes, node.NameStrOffset);
				}
			}
		}

		private static string GetTypeName(Dictionary<uint, string> customTypes, uint value)
		{
			bool isCustomType = (value & 0x80000000) == 0;
			if (isCustomType)
			{
				return customTypes[value];
			}
			else
			{
				uint offset = value & ~0x80000000;
				TreeNodeType nodeType = (TreeNodeType)offset;
				if (!Enum.IsDefined(typeof(TreeNodeType), nodeType))
				{
					throw new Exception($"Unsupported asset class type name '{nodeType}''");
				}
				return nodeType.ToTypeString();
			}
		}
	}
}
