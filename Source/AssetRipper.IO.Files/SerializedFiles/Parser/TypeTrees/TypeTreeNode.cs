using AssetRipper.IO.Files.SerializedFiles.IO;
using System.Text;

namespace AssetRipper.IO.Files.SerializedFiles.Parser.TypeTrees;

public class TypeTreeNode : ISerializedReadable, ISerializedWritable
{
	/// <summary>
	/// 5.0.0a1 and greater<br/>
	/// Generation 10
	/// </summary>
	public static bool IsFormat5(FormatVersion generation) => generation >= FormatVersion.Unknown_10;
	/// <summary>
	/// 2019.1 and greater<br/>
	/// Generation 19
	/// </summary>
	public static bool HasRefTypeHash(FormatVersion generation) => generation >= FormatVersion.TypeTreeNodeWithTypeFlags;

	public TypeTreeNode() { }

	public TypeTreeNode(string type, string name, int level, bool align)
	{
		Type = type;
		Name = name;
		Level = (byte)level;
		MetaFlag = align ? TransferMetaFlags.AlignBytes : TransferMetaFlags.NoTransferFlags;
	}

	public TypeTreeNode(string type, string name, int level, int byteSize, int index, int version, int typeFlags, TransferMetaFlags metaFlag)
	{
		Type = type;
		Name = name;
		Level = (byte)level;
		ByteSize = byteSize;
		Index = index;
		Version = version;
		TypeFlags = typeFlags;
		MetaFlag = metaFlag;
	}

	public void Read(SerializedReader reader)
	{
		if (IsFormat5(reader.Generation))
		{
			Version = reader.ReadUInt16();
			Level = reader.ReadByte();
			TypeFlags = reader.ReadByte();
			TypeStrOffset = reader.ReadUInt32();
			NameStrOffset = reader.ReadUInt32();
			ByteSize = reader.ReadInt32();
			Index = reader.ReadInt32();
			MetaFlag = (TransferMetaFlags)reader.ReadUInt32();
			if (HasRefTypeHash(reader.Generation))
			{
				RefTypeHash = reader.ReadUInt64();
			}
		}
		else
		{
			Type = reader.ReadStringZeroTerm();
			Name = reader.ReadStringZeroTerm();
			ByteSize = reader.ReadInt32();
			Index = reader.ReadInt32();
			TypeFlags = reader.ReadInt32();
			Version = reader.ReadInt32();
			MetaFlag = (TransferMetaFlags)reader.ReadUInt32();
		}
	}

	public void Write(SerializedWriter writer)
	{
		if (IsFormat5(writer.Generation))
		{
			writer.Write((ushort)Version);
			writer.Write(Level);
			writer.Write((byte)TypeFlags);
			writer.Write(TypeStrOffset);
			writer.Write(NameStrOffset);
			writer.Write(ByteSize);
			writer.Write(Index);
			writer.Write((uint)MetaFlag);
			if (HasRefTypeHash(writer.Generation))
			{
				writer.Write(RefTypeHash);
			}
		}
		else
		{
			writer.WriteStringZeroTerm(Type);
			writer.WriteStringZeroTerm(Name);
			writer.Write(ByteSize);
			writer.Write(Index);
			writer.Write(TypeFlags);
			writer.Write(Version);
			writer.Write((uint)MetaFlag);
		}
	}

	public override string? ToString()
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

	public StringBuilder ToString(StringBuilder sb)
	{
		sb.Append('\t', Level).Append(Type).Append(' ').Append(Name);
		sb.AppendFormat(" // ByteSize{0}{1:x}{2}, Index{3}{4:x}{5}, Version{6}{7:x}{8}, IsArray{{{9}}}, MetaFlag{10}{11:x}{12}",
				"{", ByteSize, "}",
				"{", Index, "}",
				"{", Version, "}",
				TypeFlags,
				"{", (uint)MetaFlag, "}");
		return sb;
	}

	/// <summary>
	/// Field type version, starts with 1 and is incremented after the type information has been significantly updated in a new release.<br/>
	/// Equal to serializedVersion in Yaml format files
	/// </summary>
	public int Version { get; set; }
	/// <summary>
	/// Depth of current type relative to root
	/// </summary>
	public byte Level { get; set; }
	/// <summary>
	/// Array flag, set to 1 if type is "Array" or "TypelessData".
	/// </summary>
	public int TypeFlags { get; set; }
	/// <summary>
	/// Type offset in <see cref="TypeTree.StringBuffer"/>
	/// </summary>
	public uint TypeStrOffset { get; set; }
	/// <summary>
	/// Name offset in <see cref="TypeTree.StringBuffer"/>
	/// </summary>
	public uint NameStrOffset { get; set; }
	/// <summary>
	/// Name of the data type. This can be the name of any substructure or a static predefined type.
	/// </summary>
	public string Type { get; set; } = "";
	/// <summary>
	/// Name of the field.
	/// </summary>
	public string Name { get; set; } = "";
	/// <summary>
	/// Size of the data value in bytes, e.g. 4 for int. -1 means that there is an array somewhere inside its hierarchy<br/>
	/// Note: The padding for the alignment is not included in the size.
	/// </summary>
	public int ByteSize { get; set; }
	/// <summary>
	/// Index of the field that is unique within a tree.<br/>
	/// Normally starts with 0 and is incremented with each additional field.
	/// </summary>
	public int Index { get; set; }
	/// <summary>
	/// Metaflags of the field
	/// </summary>
	public TransferMetaFlags MetaFlag { get; set; }
	public ulong RefTypeHash { get; set; }
}
