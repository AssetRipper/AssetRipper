namespace AssetRipper.IO.Files.SourceGenerator.Json;

internal sealed class VersionIdentifier
{
	/// <summary>
	/// The byte offset of the version number.
	/// </summary>
	public int Offset { get; set; }
	/// <summary>
	/// Is the version number big endian or little endian?
	/// </summary>
	public bool BigEndian { get; set; }
	/// <summary>
	/// The name of the enum to generate.
	/// </summary>
	public string EnumName { get; set; } = string.Empty;
	/// <summary>
	/// Must be a primitive integer.
	/// </summary>
	public string EnumType { get; set; } = string.Empty;
	/// <summary>
	/// The fields for the generated enumeration.
	/// </summary>
	/// <remarks>
	/// The fields must be one-to-one.
	/// </remarks>
	public List<EnumField> Fields { get; set; } = new();
}
