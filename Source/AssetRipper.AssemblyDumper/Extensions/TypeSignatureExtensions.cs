namespace AssetRipper.AssemblyDumper.Extensions;

internal static class TypeSignatureExtensions
{
	public static bool IsUtf8String(this TypeSignature signature)
	{
		return signature is TypeDefOrRefSignature { Name: nameof(Utf8String) } signature2 && signature2.Namespace == typeof(Utf8String).Namespace;
	}
	public static bool IsIntegerPrimitive(this TypeSignature signature, out ElementType elementType)
	{
		if (signature is CorLibTypeSignature corLibTypeSignature)
		{
			elementType = corLibTypeSignature.ElementType;
			return elementType is
				ElementType.U1 or
				ElementType.U2 or
				ElementType.U4 or
				ElementType.U or
				ElementType.U8 or
				ElementType.I1 or
				ElementType.I2 or
				ElementType.I4 or
				ElementType.I or
				ElementType.I8;
		}
		else
		{
			elementType = default;
			return false;
		}
	}
}
