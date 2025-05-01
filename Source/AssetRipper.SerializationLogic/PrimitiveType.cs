using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AssetRipper.SerializationLogic;

public enum PrimitiveType
{
	Void,
	Bool,
	Char,
	SByte,
	Byte,
	Short,
	UShort,
	Int,
	UInt,
	Long,
	ULong,
	Single,
	Double,
	String,
	Pair,
	MapPair,
	Complex,
}
public static class PrimitiveTypeExtensions
{
	public static bool IsCSharpPrimitive(this PrimitiveType _this)
	{
		return _this is PrimitiveType.String || _this.GetSize() > 0;
	}

	public static int GetSize(this PrimitiveType _this)
	{
		switch (_this)
		{
			case PrimitiveType.Void:
				return 0;

			case PrimitiveType.Bool:
			case PrimitiveType.Byte:
			case PrimitiveType.SByte:
				return 1;

			case PrimitiveType.Char:
			case PrimitiveType.Short:
			case PrimitiveType.UShort:
				return 2;

			case PrimitiveType.Int:
			case PrimitiveType.UInt:
			case PrimitiveType.Single:
				return 4;

			case PrimitiveType.Long:
			case PrimitiveType.ULong:
			case PrimitiveType.Double:
				return 8;

			case PrimitiveType.Pair:
			case PrimitiveType.MapPair:
			case PrimitiveType.String:
			case PrimitiveType.Complex:
				return -1;

			default:
				throw new NotSupportedException();
		}
	}

	public static string ToSystemTypeName(this PrimitiveType _this)
	{
		return _this switch
		{
			PrimitiveType.Bool => nameof(Boolean),
			PrimitiveType.Char => nameof(Char),
			PrimitiveType.Byte => nameof(Byte),
			PrimitiveType.SByte => nameof(SByte),
			PrimitiveType.Short => nameof(Int16),
			PrimitiveType.UShort => nameof(UInt16),
			PrimitiveType.Int => nameof(Int32),
			PrimitiveType.UInt => nameof(UInt32),
			PrimitiveType.Long => nameof(Int64),
			PrimitiveType.ULong => nameof(UInt64),
			PrimitiveType.Single => nameof(Single),
			PrimitiveType.Double => nameof(Double),
			PrimitiveType.String => nameof(String),
			_ => throw new NotSupportedException(),
		};
	}

	public static PrimitiveType ToPrimitiveType(this CorLibTypeSignature type)
	{
		return type.ElementType.ToPrimitiveType();
	}

	public static PrimitiveType ToPrimitiveType(this ElementType elementType)
	{
		return elementType switch
		{
			ElementType.Boolean => PrimitiveType.Bool,
			ElementType.Char => PrimitiveType.Char,
			ElementType.I1 => PrimitiveType.SByte,
			ElementType.U1 => PrimitiveType.Byte,
			ElementType.I2 => PrimitiveType.Short,
			ElementType.U2 => PrimitiveType.UShort,
			ElementType.I4 => PrimitiveType.Int,
			ElementType.U4 => PrimitiveType.UInt,
			ElementType.I8 => PrimitiveType.Long,
			ElementType.U8 => PrimitiveType.ULong,
			ElementType.R4 => PrimitiveType.Single,
			ElementType.R8 => PrimitiveType.Double,
			ElementType.String => PrimitiveType.String,
			_ => PrimitiveType.Complex,
		};
	}
}
