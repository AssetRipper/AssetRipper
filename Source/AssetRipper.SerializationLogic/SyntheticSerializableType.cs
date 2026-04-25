namespace AssetRipper.SerializationLogic;

public sealed class SyntheticSerializableType : SerializableType
{
	public SyntheticSerializableType(string? @namespace, PrimitiveType type, string name, IReadOnlyList<Field> fields, int version = 1, bool flowMappedInYaml = false) : base(@namespace, type, name)
	{
		Fields = fields;
		Version = version;
		FlowMappedInYaml = flowMappedInYaml;
		MaxDepth = CalculateMaxDepth(fields);
	}

	public override int Version { get; }
	public override bool FlowMappedInYaml { get; }

	private static int CalculateMaxDepth(IReadOnlyList<Field> fields)
	{
		int maxDepth = 0;
		foreach (Field field in fields)
		{
			if (field.Type.IsMaxDepthKnown)
			{
				maxDepth = Math.Max(maxDepth, field.Type.MaxDepth + 1);
			}
		}
		return maxDepth;
	}
}
