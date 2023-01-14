namespace AssetRipper.IO.Files.SourceGenerator.Json;

internal sealed class FormatDeclaration
{
	/// <summary>
	/// The name of this format.
	/// </summary>
	/// <remarks>
	/// This name will serve as the identifier for this format in both other json files and generated source files.
	/// </remarks>
	public string Name { get; set; } = string.Empty;
	/// <summary>
	/// The sub-namespace for this format.
	/// </summary>
	public string Namespace { get; set; } = string.Empty;
	/// <summary>
	/// The magic bytes identifying this format.
	/// </summary>
	/// <remarks>
	/// Only entries of length 1 or 2 are allowed. Length 1 strings are treated as Ascii. Length 2 strings are treated as hexidecimals.
	/// </remarks>
	public string[] Magic { get; set; } = Array.Empty<string>();
	/// <summary>
	/// The size and offset of this format's version enum.
	/// </summary>
	/// <remarks>
	/// Null indicates that this format doesn't include a version number.
	/// </remarks>
	public VersionIdentifier? VersionIdentifier { get; set; }
}
