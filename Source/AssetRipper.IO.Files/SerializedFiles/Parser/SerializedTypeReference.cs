using AssetRipper.IO.Files.SerializedFiles.IO;

namespace AssetRipper.IO.Files.SerializedFiles.Parser;

/// <summary>
/// A reference type for a serializeable C# type.
/// </summary>
/// <remarks>
/// These are used for fields with the [SerializeReference] attribute.
/// </remarks>
public sealed class SerializedTypeReference : SerializedTypeBase, IEquatable<SerializedTypeReference?>
{
	public Utf8String ClassName { get; set; } = Utf8String.Empty;
	public Utf8String Namespace { get; set; } = Utf8String.Empty;
	public Utf8String AsmName { get; set; } = Utf8String.Empty;

	public string FullName
	{
		get
		{
			return Utf8String.IsNullOrEmpty(Namespace)
				? ClassName
				: $"{Namespace}.{ClassName}";
		}
	}

	protected override bool IgnoreScriptTypeForHash(FormatVersion formatVersion, UnityVersion unityVersion)
	{
		return false;
	}

	private protected override void ReadTypeDependencies(SerializedReader reader)
	{
		ClassName = reader.ReadStringZeroTerm();
		Namespace = reader.ReadStringZeroTerm();
		AsmName = reader.ReadStringZeroTerm();
	}

	private protected override void WriteTypeDependencies(SerializedWriter writer)
	{
		writer.WriteStringZeroTerm(ClassName);
		writer.WriteStringZeroTerm(Namespace);
		writer.WriteStringZeroTerm(AsmName);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as SerializedTypeReference);
	}

	public bool Equals(SerializedTypeReference? other)
	{
		return other is not null
			&& RawTypeID == other.RawTypeID
			&& IsStrippedType == other.IsStrippedType
			&& ScriptTypeIndex == other.ScriptTypeIndex
			&& OldType.Equals(other.OldType)
			&& ScriptID == other.ScriptID
			&& OldTypeHash == other.OldTypeHash
			&& ClassName == other.ClassName
			&& Namespace == other.Namespace
			&& AsmName == other.AsmName;
	}

	public override int GetHashCode()
	{
		HashCode hash = new();
		hash.Add(RawTypeID);
		hash.Add(IsStrippedType);
		hash.Add(ScriptTypeIndex);
		hash.Add(OldType);
		hash.Add(ScriptID);
		hash.Add(OldTypeHash);
		hash.Add(ClassName);
		hash.Add(Namespace);
		hash.Add(AsmName);
		return hash.ToHashCode();
	}

	public static bool operator ==(SerializedTypeReference? left, SerializedTypeReference? right)
	{
		return EqualityComparer<SerializedTypeReference>.Default.Equals(left, right);
	}

	public static bool operator !=(SerializedTypeReference? left, SerializedTypeReference? right)
	{
		return !(left == right);
	}
}
