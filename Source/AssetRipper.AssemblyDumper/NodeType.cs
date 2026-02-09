namespace AssetRipper.AssemblyDumper;

public enum NodeType
{
	Type,
	Boolean,
	Character,
	String,
	Int8,
	Int16,
	Int32,
	Int64,
	UInt8,
	UInt16,
	UInt32,
	UInt64,
	Single,
	Double,
	Vector,
	Array,
	Pair,
	Map,
	TypelessData,
}

public static class NodeTypeExtensions
{
	public static string ToPrimitiveTypeName(this NodeType type)
	{
		return type switch
		{
			NodeType.Boolean => nameof(Boolean),
			NodeType.Character => nameof(Char),
			NodeType.Int8 => nameof(SByte),
			NodeType.UInt8 => nameof(Byte),
			NodeType.Int16 => nameof(Int16),
			NodeType.UInt16 => nameof(UInt16),
			NodeType.Int32 => nameof(Int32),
			NodeType.UInt32 => nameof(UInt32),
			NodeType.Int64 => nameof(Int64),
			NodeType.UInt64 => nameof(UInt64),
			NodeType.Single => nameof(Single),
			NodeType.Double => nameof(Double),
			NodeType.String => nameof(Utf8String),
			_ => throw new NotSupportedException(type.ToString()),
		};
	}

	public static bool IsPrimitive(this NodeType type)
	{
		return type is
			NodeType.Boolean or
			NodeType.Character or
			NodeType.Int8 or
			NodeType.UInt8 or
			NodeType.Int16 or
			NodeType.UInt16 or
			NodeType.Int32 or
			NodeType.UInt32 or
			NodeType.Int64 or
			NodeType.UInt64 or
			NodeType.Single or
			NodeType.Double or
			NodeType.String;
	}

	public static TypeSignature ToPrimitiveTypeSignature(this NodeType type)
	{
		return type switch
		{
			NodeType.Boolean => SharedState.Instance.Importer.Boolean,
			NodeType.Character => SharedState.Instance.Importer.Char,
			NodeType.Int8 => SharedState.Instance.Importer.Int8,
			NodeType.Int16 => SharedState.Instance.Importer.Int16,
			NodeType.Int32 => SharedState.Instance.Importer.Int32,
			NodeType.Int64 => SharedState.Instance.Importer.Int64,
			NodeType.UInt8 => SharedState.Instance.Importer.UInt8,
			NodeType.UInt16 => SharedState.Instance.Importer.UInt16,
			NodeType.UInt32 => SharedState.Instance.Importer.UInt32,
			NodeType.UInt64 => SharedState.Instance.Importer.UInt64,
			NodeType.Single => SharedState.Instance.Importer.Single,
			NodeType.Double => SharedState.Instance.Importer.Double,
			NodeType.String => SharedState.Instance.Importer.ImportType<Utf8String>().ToTypeSignature(),
			_ => throw new NotSupportedException(type.ToString()),
		};
	}
}
