namespace AssetRipper.SerializationLogic;

internal sealed class CustomSerializableType : SerializableType
{
	public CustomSerializableType(string? @namespace, PrimitiveType type, string name, IReadOnlyList<SerializableType.Field> fields) : base(@namespace, type, name)
	{
		Fields = fields;
		MaxDepth = 0;
		foreach (Field field in Fields)
		{
			if (field.Type.IsMaxDepthKnown)
			{
				MaxDepth = int.Max(MaxDepth, field.Type.MaxDepth + 1);
			}
		}
	}
}
