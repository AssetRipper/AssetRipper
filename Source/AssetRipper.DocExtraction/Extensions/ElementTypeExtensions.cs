using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AssetRipper.DocExtraction.Extensions;

public static class ElementTypeExtensions
{
	public static int GetByteSize(this ElementType elementType)
	{
		return elementType switch
		{
			ElementType.I1 => 1,
			ElementType.U1 => 1,
			ElementType.I2 => 2,
			ElementType.U2 => 2,
			ElementType.I4 => 4,
			ElementType.U4 => 4,
			ElementType.I8 => 8,
			ElementType.U8 => 8,
			_ => throw new ArgumentOutOfRangeException(nameof(elementType)),
		};
	}

	public static bool IsSigned(this ElementType elementType)
	{
		return elementType switch
		{
			ElementType.I1 => true,
			ElementType.U1 => false,
			ElementType.I2 => true,
			ElementType.U2 => false,
			ElementType.I4 => true,
			ElementType.U4 => false,
			ElementType.I8 => true,
			ElementType.U8 => false,
			_ => throw new ArgumentOutOfRangeException(nameof(elementType)),
		};
	}

	public static bool IsFixedSizeInteger(this ElementType elementType)
	{
		return elementType switch
		{
			ElementType.I1 => true,
			ElementType.U1 => true,
			ElementType.I2 => true,
			ElementType.U2 => true,
			ElementType.I4 => true,
			ElementType.U4 => true,
			ElementType.I8 => true,
			ElementType.U8 => true,
			_ => false,
		};
	}

	public static bool IsUnsigned(this ElementType elementType) => !elementType.IsSigned();

	public static ElementType Merge(this ElementType first, ElementType second)
	{
		if (first == second)
		{
			return first;
		}

		int firstByteSize = first.GetByteSize();
		int secondByteSize = second.GetByteSize();

		if (firstByteSize == secondByteSize) //Different signs
		{
			return first.IncreaseToNextSignedType();
		}
		else
		{
			ElementType larger;
			ElementType smaller;
			if (firstByteSize > secondByteSize)
			{
				larger = first;
				smaller = second;
			}
			else
			{
				larger = second;
				smaller = first;
			}

			if (larger.IsSigned() || smaller.IsUnsigned())
			{
				return larger;
			}
			else
			{
				return larger.IncreaseToNextSignedType();
			}
		}
	}

	private static ElementType IncreaseToNextSignedType(this ElementType elementType)
	{
		return elementType switch
		{
			ElementType.I1 => ElementType.I2,
			ElementType.U1 => ElementType.I2,
			ElementType.I2 => ElementType.I4,
			ElementType.U2 => ElementType.I4,
			ElementType.I4 => ElementType.I8,
			ElementType.U4 => ElementType.I8,
			ElementType.I8 => throw new ArgumentOutOfRangeException(nameof(elementType), "There is no larger ElementType than I8."),
			ElementType.U8 => throw new ArgumentOutOfRangeException(nameof(elementType), "There is no larger ElementType than U8."),
			_ => throw new ArgumentOutOfRangeException(nameof(elementType)),
		};
	}
}