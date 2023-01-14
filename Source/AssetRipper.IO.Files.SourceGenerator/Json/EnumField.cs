namespace AssetRipper.IO.Files.SourceGenerator.Json;

internal sealed class EnumField
{
	string Name { get; set; } = string.Empty;
	long Value { get; set; }
	string? Summary { get; set; }
}
