using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AssetRipper.DocExtraction.Extensions;

public static class ConstantExtensions
{
	public static long ConvertToLong(this Constant constant)
	{
		return constant.Type switch
		{
			ElementType.U1 => constant.InterpretData<byte>(),
			ElementType.U2 => constant.InterpretData<ushort>(),
			ElementType.U4 => constant.InterpretData<uint>(),
			ElementType.U8 => unchecked((long)constant.InterpretData<ulong>()),
			ElementType.I1 => constant.InterpretData<sbyte>(),
			ElementType.I2 => constant.InterpretData<short>(),
			ElementType.I4 => constant.InterpretData<int>(),
			ElementType.I8 => constant.InterpretData<long>(),
			_ => throw new NotSupportedException(constant.Type.ToString())
		};
	}

	public static object InterpretData(this Constant constant)
	{
		return constant.Value!.InterpretData(constant.Type);
	}

	public static T InterpretData<T>(this Constant constant)
	{
		return (T)constant.InterpretData();
	}
}