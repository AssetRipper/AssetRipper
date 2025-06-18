using AssetRipper.Primitives;

namespace AssetRipper.IO.Files.SourceGenerator;

internal static class PrimitiveHandler
{
	private static int GetByteSize(string type)
	{
		return type switch
		{
			"sbyte" or "byte" => 1,
			"short" or "ushort" or "Half" => 2,
			"int" or "uint" or "float" => 4,
			"long" or "ulong" or "double" => 8,
			"decimal" => 16,
			"nint" or "nuint" => throw new NotSupportedException(),
			_ => throw new ArgumentOutOfRangeException(nameof(type)),
		};
	}

	public static string GetCommonType(string type1, string type2)
	{
		if (type1 == type2)
		{
			return type1;
		}
		else if (IsInteger(type1) && IsInteger(type2))
		{
			int size1 = GetByteSize(type1);
			int size2 = GetByteSize(type2);

			if (size1 == size2)//different sign
			{
				string signedType = IsSignedInteger(type1) ? type1 : type2;
				return signedType switch
				{
					"sbyte" => "short",
					"short" => "int",
					"int" => "long",
					"long" => throw new NotSupportedException(),
					_ => throw new ArgumentOutOfRangeException(),
				};
			}
			else if (IsSignedInteger(type1) || IsSignedInteger(type2))
			{
				string largerType = size1 > size2 ? type1 : type2;
				return largerType switch
				{
					"short" => "short",
					"ushort" or "int" => "int",
					"uint" or "long" => "long",
					"ulong" => throw new NotSupportedException(),
					_ => throw new ArgumentOutOfRangeException(),
				};
			}
			else
			{
				string largerType = size1 > size2 ? type1 : type2;
				return largerType;
			}
		}
		else if (IsFloatingPoint(type1) && IsFloatingPoint(type2))
		{
			int size1 = GetByteSize(type1);
			int size2 = GetByteSize(type2);
			string largerType = size1 > size2 ? type1 : type2;
			return largerType;
		}
		else
		{
			throw new NotSupportedException();
		}
	}

	public static bool IsInteger(string type) => IsSignedInteger(type) || IsUnsignedInteger(type);

	public static bool IsSignedInteger(string type) => type is "sbyte" or "short" or "int" or "nint" or "long";

	public static bool IsUnsignedInteger(string type) => type is "byte" or "ushort" or "uint" or "nuint" or "ulong";

	public static bool IsFloatingPoint(string type) => type is "Half" or "float" or "double" or "decimal";

	public static bool IsNumber(string type) => IsInteger(type) || IsFloatingPoint(type);

	public static bool GetTypeNameForKeyword(string keyword, [NotNullWhen(true)] out string? typeName)
	{
		typeName = keyword switch
		{
			"bool" => nameof(Boolean),
			"char" => nameof(Char),
			"sbyte" => nameof(SByte),
			"byte" => nameof(Byte),
			"short" => nameof(Int16),
			"ushort" => nameof(UInt16),
			"int" => nameof(Int32),
			"uint" => nameof(UInt32),
			"long" => nameof(Int64),
			"ulong" => nameof(UInt64),
			"nint" => nameof(IntPtr),
			"nuint" => nameof(UIntPtr),
			"float" => nameof(Single),
			"double" => nameof(Double),
			"decimal" => nameof(Decimal),
			"string" => nameof(String),
			nameof(UnityGuid) => nameof(UnityGuid),
			_ => null
		};
		return typeName is not null;
	}
}
