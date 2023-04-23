using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser.TypeTrees;
using System.Collections.Specialized;
using System.Text;

namespace AssetRipper.Import.Utils
{
	public static class TypeTreeHelper
	{
		public static string ReadTypeString(TypeTree m_Type, BinaryReader reader)
		{
			StringBuilder? sb = new StringBuilder();
			List<TypeTreeNode>? m_Nodes = m_Type.Nodes;
			for (int i = 0; i < m_Nodes.Count; i++)
			{
				ReadStringValue(sb, m_Nodes, reader, ref i);
			}
			return sb.ToString();
		}

		private static void ReadStringValue(StringBuilder sb, List<TypeTreeNode> m_Nodes, BinaryReader reader, ref int i)
		{
			TypeTreeNode? m_Node = m_Nodes[i];
			byte level = m_Node.Level;
			string? varTypeStr = m_Node.Type;
			string? varNameStr = m_Node.Name;
			object? value = null;
			bool append = true;
			bool align = m_Node.MetaFlag.IsAlignBytes();
			switch (varTypeStr)
			{
				case "SInt8":
					value = reader.ReadSByte();
					break;
				case "UInt8":
				case "char":
					value = reader.ReadByte();
					break;
				case "short":
				case "SInt16":
					value = reader.ReadInt16();
					break;
				case "UInt16":
				case "unsigned short":
					value = reader.ReadUInt16();
					break;
				case "int":
				case "SInt32":
					value = reader.ReadInt32();
					break;
				case "UInt32":
				case "unsigned int":
				case "Type*":
					value = reader.ReadUInt32();
					break;
				case "long long":
				case "SInt64":
					value = reader.ReadInt64();
					break;
				case "UInt64":
				case "unsigned long long":
				case "FileSize":
					value = reader.ReadUInt64();
					break;
				case "float":
					value = reader.ReadSingle();
					break;
				case "double":
					value = reader.ReadDouble();
					break;
				case "bool":
					value = reader.ReadBoolean();
					break;
				case "string":
					append = false;
					string? str = reader.ReadAlignedString();
					sb.AppendFormat("{0}{1} {2} = \"{3}\"\r\n", new string('\t', level), varTypeStr, varNameStr, str);
					i += 3;
					break;
				case "map":
					{
						if (m_Nodes[i + 1].MetaFlag.IsAlignBytes())
						{
							align = true;
						}

						append = false;
						sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level), varTypeStr, varNameStr);
						sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level + 1), "Array", "Array");
						int size = reader.ReadInt32();
						sb.AppendFormat("{0}{1} {2} = {3}\r\n", new string('\t', level + 1), "int", "size", size);
						List<TypeTreeNode>? map = GetNodes(m_Nodes, i);
						i += map.Count - 1;
						List<TypeTreeNode>? first = GetNodes(map, 4);
						int next = 4 + first.Count;
						List<TypeTreeNode>? second = GetNodes(map, next);
						for (int j = 0; j < size; j++)
						{
							sb.AppendFormat("{0}[{1}]\r\n", new string('\t', level + 2), j);
							sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level + 2), "pair", "data");
							int tmp1 = 0;
							int tmp2 = 0;
							ReadStringValue(sb, first, reader, ref tmp1);
							ReadStringValue(sb, second, reader, ref tmp2);
						}
						break;
					}
				case "TypelessData":
					{
						append = false;
						int size = reader.ReadInt32();
						reader.ReadBytes(size);
						i += 2;
						sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level), varTypeStr, varNameStr);
						sb.AppendFormat("{0}{1} {2} = {3}\r\n", new string('\t', level), "int", "size", size);
						break;
					}
				default:
					{
						if (i < m_Nodes.Count - 1 && m_Nodes[i + 1].Type == "Array") //Array
						{
							if (m_Nodes[i + 1].MetaFlag.IsAlignBytes())
							{
								align = true;
							}

							append = false;
							sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level), varTypeStr, varNameStr);
							sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level + 1), "Array", "Array");
							int size = reader.ReadInt32();
							sb.AppendFormat("{0}{1} {2} = {3}\r\n", new string('\t', level + 1), "int", "size", size);
							List<TypeTreeNode>? vector = GetNodes(m_Nodes, i);
							i += vector.Count - 1;
							for (int j = 0; j < size; j++)
							{
								sb.AppendFormat("{0}[{1}]\r\n", new string('\t', level + 2), j);
								int tmp = 3;
								ReadStringValue(sb, vector, reader, ref tmp);
							}
							break;
						}
						else //Class
						{
							append = false;
							sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level), varTypeStr, varNameStr);
							List<TypeTreeNode>? @class = GetNodes(m_Nodes, i);
							i += @class.Count - 1;
							for (int j = 1; j < @class.Count; j++)
							{
								ReadStringValue(sb, @class, reader, ref j);
							}
							break;
						}
					}
			}
			if (append)
			{
				sb.AppendFormat("{0}{1} {2} = {3}\r\n", new string('\t', level), varTypeStr, varNameStr, value);
			}

			if (align)
			{
				reader.AlignStream();
			}
		}

		public static OrderedDictionary ReadType(TypeTree m_Types, BinaryReader reader)
		{
			OrderedDictionary? obj = new OrderedDictionary();
			List<TypeTreeNode>? m_Nodes = m_Types.Nodes;
			for (int i = 1; i < m_Nodes.Count; i++)
			{
				TypeTreeNode? m_Node = m_Nodes[i];
				string? varNameStr = m_Node.Name;
				obj[varNameStr] = ReadValue(m_Nodes, reader, ref i);
			}
			return obj;
		}

		private static object ReadValue(List<TypeTreeNode> m_Nodes, BinaryReader reader, ref int i)
		{
			TypeTreeNode? m_Node = m_Nodes[i];
			string? varTypeStr = m_Node.Type;
			object value;
			bool align = m_Node.MetaFlag.IsAlignBytes();
			switch (varTypeStr)
			{
				case "SInt8":
					value = reader.ReadSByte();
					break;
				case "UInt8":
				case "char":
					value = reader.ReadByte();
					break;
				case "short":
				case "SInt16":
					value = reader.ReadInt16();
					break;
				case "UInt16":
				case "unsigned short":
					value = reader.ReadUInt16();
					break;
				case "int":
				case "SInt32":
					value = reader.ReadInt32();
					break;
				case "UInt32":
				case "unsigned int":
				case "Type*":
					value = reader.ReadUInt32();
					break;
				case "long long":
				case "SInt64":
					value = reader.ReadInt64();
					break;
				case "UInt64":
				case "unsigned long long":
				case "FileSize":
					value = reader.ReadUInt64();
					break;
				case "float":
					value = reader.ReadSingle();
					break;
				case "double":
					value = reader.ReadDouble();
					break;
				case "bool":
					value = reader.ReadBoolean();
					break;
				case "string":
					value = reader.ReadAlignedString();
					i += 3;
					break;
				case "map":
					{
						if (m_Nodes[i + 1].MetaFlag.IsAlignBytes())
						{
							align = true;
						}

						List<TypeTreeNode>? map = GetNodes(m_Nodes, i);
						i += map.Count - 1;
						List<TypeTreeNode>? first = GetNodes(map, 4);
						int next = 4 + first.Count;
						List<TypeTreeNode>? second = GetNodes(map, next);
						int size = reader.ReadInt32();
						List<KeyValuePair<object, object>>? dic = new List<KeyValuePair<object, object>>(size);
						for (int j = 0; j < size; j++)
						{
							int tmp1 = 0;
							int tmp2 = 0;
							dic.Add(new KeyValuePair<object, object>(ReadValue(first, reader, ref tmp1), ReadValue(second, reader, ref tmp2)));
						}
						value = dic;
						break;
					}
				case "TypelessData":
					{
						int size = reader.ReadInt32();
						value = reader.ReadBytes(size);
						i += 2;
						break;
					}
				default:
					{
						if (i < m_Nodes.Count - 1 && m_Nodes[i + 1].Type == "Array") //Array
						{
							if (m_Nodes[i + 1].MetaFlag.IsAlignBytes())
							{
								align = true;
							}

							List<TypeTreeNode>? vector = GetNodes(m_Nodes, i);
							i += vector.Count - 1;
							int size = reader.ReadInt32();
							List<object>? list = new List<object>(size);
							for (int j = 0; j < size; j++)
							{
								int tmp = 3;
								list.Add(ReadValue(vector, reader, ref tmp));
							}
							value = list;
							break;
						}
						else //Class
						{
							List<TypeTreeNode>? @class = GetNodes(m_Nodes, i);
							i += @class.Count - 1;
							OrderedDictionary? obj = new OrderedDictionary();
							for (int j = 1; j < @class.Count; j++)
							{
								TypeTreeNode? classmember = @class[j];
								string? name = classmember.Name;
								obj[name] = ReadValue(@class, reader, ref j);
							}
							value = obj;
							break;
						}
					}
			}
			if (align)
			{
				reader.AlignStream();
			}

			return value;
		}

		private static List<TypeTreeNode> GetNodes(List<TypeTreeNode> m_Nodes, int index)
		{
			List<TypeTreeNode>? nodes = new List<TypeTreeNode>();
			nodes.Add(m_Nodes[index]);
			byte level = m_Nodes[index].Level;
			for (int i = index + 1; i < m_Nodes.Count; i++)
			{
				TypeTreeNode? member = m_Nodes[i];
				byte level2 = member.Level;
				if (level2 <= level)
				{
					return nodes;
				}
				nodes.Add(member);
			}
			return nodes;
		}

		private static void AlignStream(this BinaryReader reader) => reader.BaseStream.Align();
		private static void AlignStream(this BinaryReader reader, int alignment) => reader.BaseStream.Align(alignment);

		private static string ReadAlignedString(this BinaryReader reader)
		{
			int length = reader.ReadInt32();
			if (length > 0 && length <= reader.BaseStream.Length - reader.BaseStream.Position)
			{
				byte[] stringData = reader.ReadBytes(length);
				string result = Encoding.UTF8.GetString(stringData);
				reader.AlignStream(4);
				return result;
			}
			return "";
		}

		private static void Align(this Stream _this) => _this.Align(4);
		private static void Align(this Stream _this, int alignment)
		{
			long pos = _this.Position;
			long mod = pos % alignment;
			if (mod != 0)
			{
				_this.Position += alignment - mod;
			}
		}
	}
}
