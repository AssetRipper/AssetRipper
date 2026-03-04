using AssetRipper.IO.Files.SerializedFiles.IO;

namespace AssetRipper.IO.Files.SerializedFiles.Parser;

/// <summary>
/// A reference type for a serializeable C# type.
/// </summary>
/// <remarks>
/// These are used for fields with the [SerializeReference] attribute.
/// </remarks>
public sealed class SerializedTypeReference : SerializedTypeBase
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
}
