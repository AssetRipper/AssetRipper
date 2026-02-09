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
	public string ClassName { get; set; } = "";
	public string Namespace { get; set; } = "";
	public string AsmName { get; set; } = "";

	public string FullName
	{
		get
		{
			return string.IsNullOrEmpty(Namespace)
				? ClassName
				: $"{Namespace}.{ClassName}";
		}
	}

	protected override bool IgnoreScriptTypeForHash(FormatVersion formatVersion, UnityVersion unityVersion)
	{
		return false;
	}

	protected override void ReadTypeDependencies(SerializedReader reader)
	{
		ClassName = reader.ReadStringZeroTerm();
		Namespace = reader.ReadStringZeroTerm();
		AsmName = reader.ReadStringZeroTerm();
	}

	protected override void WriteTypeDependencies(SerializedWriter writer)
	{
		writer.WriteStringZeroTerm(ClassName);
		writer.WriteStringZeroTerm(Namespace);
		writer.WriteStringZeroTerm(AsmName);
	}
}
