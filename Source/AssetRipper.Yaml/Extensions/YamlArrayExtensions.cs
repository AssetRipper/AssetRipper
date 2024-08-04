namespace AssetRipper.Yaml.Extensions;

public static class YamlArrayExtensions
{
	public static YamlNode ExportYaml(this IReadOnlyList<byte> _this)
	{
		return YamlScalarNode.CreateHex(_this);
	}

	public static void AddTypelessData(this YamlMappingNode mappingNode, string name, IReadOnlyList<byte> data)
	{
		mappingNode.Add(name, data.Count);
		mappingNode.Add(TypelessdataName, data.ExportYaml());
	}

	public const string TypelessdataName = "_typelessdata";
}
