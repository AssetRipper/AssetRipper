using System.Diagnostics;

namespace AssetRipper.SerializationLogic;

/// <summary>
/// A <see cref="SerializableType"/> with a hard-coded layout.
/// </summary>
internal sealed class CustomSerializableType : SerializableType
{
	public CustomSerializableType(string name, bool flowMappedInYaml, IReadOnlyList<Field> fields) : base(null, PrimitiveType.Complex, name)
	{
		FlowMappedInYaml = flowMappedInYaml;
		Fields = fields;

		int maxDepth = 0;
		foreach (Field field in fields)
		{
			Debug.Assert(field.Type.IsMaxDepthKnown, "A custom type can only contain types with a known depth.");
			maxDepth = int.Max(maxDepth, field.Type.MaxDepth + 1);
		}
		MaxDepth = maxDepth;
	}

	public override bool FlowMappedInYaml { get; }
}
