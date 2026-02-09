namespace AssetRipper.AssemblyDumper.Passes;

internal readonly struct TypeSignatureStruct : IEquatable<TypeSignatureStruct>
{
	private static readonly SignatureComparer signatureComparer = new();

	public TypeSignatureStruct(TypeSignature signature)
	{
		Signature = signature;
	}

	public TypeSignature Signature { get; }

	public override string ToString()
	{
		return Signature.ToString();
	}

	public override bool Equals(object? obj)
	{
		return (obj is TypeSignatureStruct @struct && Equals(@struct));
	}

	public bool Equals(TypeSignatureStruct other)
	{
		return signatureComparer.Equals(Signature, other.Signature);
	}

	public override int GetHashCode()
	{
		return signatureComparer.GetHashCode(Signature);
	}

	public static bool operator ==(TypeSignatureStruct left, TypeSignatureStruct right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(TypeSignatureStruct left, TypeSignatureStruct right)
	{
		return !(left == right);
	}

	public static implicit operator TypeSignature(TypeSignatureStruct signature) => signature.Signature;
	public static implicit operator TypeSignatureStruct(TypeSignature signature) => new TypeSignatureStruct(signature);
	public static implicit operator TypeSignatureStruct(TypeDefinition type) => new TypeSignatureStruct(type.ToTypeSignature());
}
