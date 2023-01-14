using System.Text.Json.Serialization;

namespace AssetRipper.IO.Files.SourceGenerator.Json;

internal sealed class TypeDefinition
{
	/// <summary>
	/// The name of this format.
	/// </summary>
	/// <remarks>
	/// This must match <see cref="TypeDeclaration.Name"/>.
	/// </remarks>
	public string Name { get; set; } = string.Empty;
	/// <summary>
	/// The specific version of this <see cref="TypeDefinition"/>.
	/// </summary>
	/// <remarks>
	/// This value must correspond to one declared in <see cref="VersionIdentifier.Fields"/>.
	/// If this is equal to one, it's assumed to be version-less.
	/// </remarks>
	public long Version { get; set; }
	public List<FieldDefinition> SerializableFields { get; set; } = new();
	public List<FieldDefinition> ExtraFields { get; set; } = new();
	[JsonIgnore]
	public IEnumerable<FieldDefinition> AllFields => SerializableFields.Union(ExtraFields);

	public override string ToString()
	{
		return Name;
	}
}
