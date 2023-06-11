namespace AssetRipper.IO.Files.SourceGenerator.Json;

internal sealed class TypeDeclaration
{
	/// <summary>
	/// The name of this type.
	/// </summary>
	/// <remarks>
	/// This name will serve as the identifier for this type in both other json files and generated source files.
	/// </remarks>
	public string Name { get; set; } = string.Empty;
	/// <summary>
	/// The namespace for this type. Must start with "AssetRipper.IO.Files."
	/// </summary>
	public string Namespace { get; set; } = string.Empty;
	/// <summary>
	/// The class type of this type. For example:
	/// <list type="bullet">
	/// <item>struct</item>
	/// <item>readonly struct</item>
	/// <item>record struct</item>
	/// <item>readonly record struct</item>
	/// <item>class</item>
	/// <item>record class</item>
	/// <item>record</item>
	/// </list>
	/// </summary>
	public string ClassType { get; set; } = string.Empty;
	//public bool GenerateBaseClass { get; set; }
	/// <summary>
	/// The summary for this type in xml documentation.
	/// </summary>
	public string? Summary { get; set; }
	/// <summary>
	/// The remarks for this type in xml documentation.
	/// </summary>
	public string? Remarks { get; set; }
	/// <summary>
	/// The magic bytes identifying this format.
	/// </summary>
	public Dictionary<string, PropertyDocumentation?> Properties { get; set; } = new();
	/// <summary>
	/// The additional usings required for this type.
	/// </summary>
	public string[] Usings { get; set; } = Array.Empty<string>();

	public override string ToString()
	{
		return Name;
	}

	public bool TryGetPropertyDocumentation(string property, out string? summary, out string? remarks)
	{
		Properties.TryGetValue(property, out PropertyDocumentation? documentation);
		if (documentation is not null)
		{
			summary = documentation.Summary;
			remarks = documentation.Remarks;
			return !string.IsNullOrEmpty(summary) || !string.IsNullOrEmpty(remarks);
		}
		else
		{
			summary = null;
			remarks = null;
			return false;
		}
	}

	public bool ContainsModifier(string modifier)
	{
		return ClassType.Contains(modifier, StringComparison.Ordinal);
	}
}
