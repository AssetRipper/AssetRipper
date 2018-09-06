using System;

namespace UtinyRipper.SerializedFiles
{
	internal sealed class TypeTreeNode
	{
		public TypeTreeNode()
		{
		}

		public TypeTreeNode(byte depth)
		{
			Depth = depth;
		}

		public void Read(SerializedFileReader reader)
		{
			Type = reader.ReadStringZeroTerm();
			Name = reader.ReadStringZeroTerm();
			ByteSize = reader.ReadInt32();
			Index = reader.ReadInt32();
			IsArray = reader.ReadInt32() != 0;
			Version = reader.ReadInt32();
			MetaFlag = reader.ReadUInt32();
		}

		public void Read(SerializedFileReader reader, long stringPosition)
		{
			Version = reader.ReadUInt16();
			Depth = reader.ReadByte();
			IsArray = reader.ReadBoolean();
			uint type = reader.ReadUInt32();
			uint name = reader.ReadUInt32();
			ByteSize = reader.ReadInt32();
			Index = reader.ReadInt32();
			MetaFlag = reader.ReadUInt32();

			Type = ReadString(reader, stringPosition, type);
			Name = ReadString(reader, stringPosition, name);
		}

		private static string ReadString(SerializedFileReader reader, long stringPosition, uint value)
		{
			bool isCustomType = (value & 0x80000000) == 0;
			if (isCustomType)
			{
				long position = reader.BaseStream.Position;
				reader.BaseStream.Position = stringPosition + value;
				string stringValue = reader.ReadStringZeroTerm();
				reader.BaseStream.Position = position;
				return stringValue;
			}
			else
			{
				uint type = value & 0x7FFFFFFF;
				TreeNodeType nodeType = (TreeNodeType)type;
				if (!Enum.IsDefined(typeof(TreeNodeType), nodeType))
				{
					throw new Exception($"Unsupported asset class type name '{nodeType}''");
				}
				return nodeType.ToTypeString();
			}
		}

		public override string ToString()
		{
			if (Type == null)
			{
				return base.ToString();
			}
			else
			{
				return $"{Type} {Name}";
			}
		}

		public const int NodeSize = 24;

		/// <summary>
		/// Field type version, starts with 1 and is incremented after the type information has been significantly updated in a new release.
		/// Equal to serializedVersion in YAML format files
		/// </summary>
		public int Version { get; private set; }
		/// <summary>
		/// Depth of current type relative to root
		/// </summary>
		public byte Depth { get; private set; }
		/// <summary>
		/// Array flag, set to 1 if type is "Array" or "TypelessData".
		/// </summary>
		public bool IsArray { get; private set; }
		/// <summary>
		/// Name of the data type. This can be the name of any substructure or a static predefined type.
		/// </summary>
		public string Type { get; private set; }
		/// <summary>
		/// Name of the field.
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// Size of the data value in bytes, e.g. 4 for int. -1 means that the field is a class and contains child fields only.
		/// Note: The padding for the alignment is not included in the size.
		/// </summary>
		public int ByteSize { get; private set; }
		/// <summary>
		/// Index of the field that is unique within a tree.
		/// Normally starts with 0 and is incremented with each additional field.
		/// </summary>
		public int Index { get; private set; }
		/// <summary>
		/// Metaflags of the field. Purpose is mostly unknown.
		/// </summary>
		public uint MetaFlag { get; private set; }
	}
}
