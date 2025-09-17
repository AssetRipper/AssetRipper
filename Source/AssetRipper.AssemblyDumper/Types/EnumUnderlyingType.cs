namespace AssetRipper.AssemblyDumper.Types;

public enum EnumUnderlyingType : byte
{
	/// <summary>
	/// sbyte
	/// </summary>
	Int8,
	/// <summary>
	/// byte
	/// </summary>
	UInt8,
	/// <summary>
	/// short
	/// </summary>
	Int16,
	/// <summary>
	/// ushort
	/// </summary>
	UInt16,
	/// <summary>
	/// int
	/// </summary>
	Int32,
	/// <summary>
	/// uint
	/// </summary>
	UInt32,
	/// <summary>
	/// long
	/// </summary>
	Int64,
	/// <summary>
	/// ulong
	/// </summary>
	UInt64,
}

public static class EnumUnderlyingTypeExtensions
{
	public static CorLibTypeSignature ToTypeSignature(this EnumUnderlyingType enumUnderlyingType, AssemblyBuilder builder)
	{
		return enumUnderlyingType.ToTypeSignature(builder.Importer);
	}

	public static CorLibTypeSignature ToTypeSignature(this EnumUnderlyingType enumUnderlyingType, CachedReferenceImporter importer)
	{
		return enumUnderlyingType switch
		{
			EnumUnderlyingType.Int8 => importer.Int8,
			EnumUnderlyingType.UInt8 => importer.UInt8,
			EnumUnderlyingType.Int16 => importer.Int16,
			EnumUnderlyingType.UInt16 => importer.UInt16,
			EnumUnderlyingType.Int32 => importer.Int32,
			EnumUnderlyingType.UInt32 => importer.UInt32,
			EnumUnderlyingType.Int64 => importer.Int64,
			EnumUnderlyingType.UInt64 => importer.UInt64,
			_ => throw new ArgumentOutOfRangeException(nameof(enumUnderlyingType)),
		};
	}

	public static EnumUnderlyingType ToEnumUnderlyingType(this Type type)
	{
		if (type.Namespace != "System")
		{
			throw new ArgumentException($"{type.Namespace} is not a valid namespace for an underlying enum type", nameof(type));
		}

		return type.Name switch
		{
			nameof(SByte) => EnumUnderlyingType.Int8,
			nameof(Byte) => EnumUnderlyingType.UInt8,
			nameof(Int16) => EnumUnderlyingType.Int16,
			nameof(UInt16) => EnumUnderlyingType.UInt16,
			nameof(Int32) => EnumUnderlyingType.Int32,
			nameof(UInt32) => EnumUnderlyingType.UInt32,
			nameof(Int64) => EnumUnderlyingType.Int64,
			nameof(UInt64) => EnumUnderlyingType.UInt64,
			_ => throw new ArgumentOutOfRangeException(nameof(type), $"{type.Name} is not a valid type name for an underlying enum type"),
		};
	}

	public static EnumUnderlyingType ToEnumUnderlyingType(this ElementType type)
	{
		return type switch
		{
			ElementType.I1 => EnumUnderlyingType.Int8,
			ElementType.U1 => EnumUnderlyingType.UInt8,
			ElementType.I2 => EnumUnderlyingType.Int16,
			ElementType.U2 => EnumUnderlyingType.UInt16,
			ElementType.I4 => EnumUnderlyingType.Int32,
			ElementType.U4 => EnumUnderlyingType.UInt32,
			ElementType.I8 => EnumUnderlyingType.Int64,
			ElementType.U8 => EnumUnderlyingType.UInt64,
			_ => throw new ArgumentOutOfRangeException(nameof(type), $"{type} is not a valid type name for an underlying enum type"),
		};
	}
}
