using System;

namespace uTinyRipper.Assembly
{
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
		Complex,
	}

	public static class PrimitiveTypeExtensions
	{
		public static int GetSize(this PrimitiveType _this)
		{
			switch (_this)
			{
				case PrimitiveType.Bool:
				case PrimitiveType.Char:
				case PrimitiveType.Byte:
				case PrimitiveType.SByte:
					return 1;

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

				case PrimitiveType.String:
				case PrimitiveType.Complex:
					return -1;

				default:
					throw new NotImplementedException();
			}
		}
	}
}
