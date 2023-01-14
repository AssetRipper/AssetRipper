namespace AssetRipper.IO.Files.SourceGenerator.Json;

internal sealed class PropertyDocumentation
{
	/// <summary>
	/// The summary for this field in xml documentation.
	/// </summary>
	public string? Summary { get; set; }
	/// <summary>
	/// The remarks for this field in xml documentation.
	/// </summary>
	public string? Remarks { get; set; }
	/// <summary>
	/// The default expression to use in a get accessor if no backing field is present. Do not include a semicolon.
	/// </summary>
	public string? GetExpression { get; set; }
	/// <summary>
	/// The default expression to use in a set accessor if no backing field is present. Do not include a semicolon.
	/// </summary>
	public string? SetExpression { get; set; }
}
