namespace AssetRipper.Yaml.Extensions;

public static class YamlArrayExtensions
{
	public static void AddTypelessData(this YamlMappingNode mappingNode, string name, IReadOnlyList<byte> data)
	{
		mappingNode.Add(name, data.Count);
		mappingNode.Add(TypelessdataName, YamlScalarNode.CreateHex(data));
	}

	public const string TypelessdataName = "_typelessdata";
}
