namespace AssetRipper.Yaml;

internal enum MetaType
{
	Yaml,
	Tag,
}

internal static class MetaTypeExtensions
{
	public static string ToStringRepresentation(this MetaType metaType)
	{
		return metaType switch
		{
			MetaType.Yaml => "YAML",
			MetaType.Tag => "TAG",
			_ => throw new ArgumentOutOfRangeException(nameof(metaType), $"Value: {metaType}"),
		};
	}
}
