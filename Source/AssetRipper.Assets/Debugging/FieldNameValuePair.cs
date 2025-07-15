using System.Diagnostics;

namespace AssetRipper.Assets.Debugging;

[DebuggerDisplay("{Value}", Name = "{Name}", Type = "{Type}")]
internal readonly struct FieldNameValuePair
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public string Name { get; }

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	public object? Value { get; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public string? ValueType => Value?.GetType().ToString();

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public string? FieldType { get; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public string Type
	{
		get
		{
			if (Value is null)
			{
				return FieldType ?? "null";
			}
			else
			{
				string valueType = Value.GetType().ToString();
				if (FieldType is null || FieldType == valueType)
				{
					return valueType;
				}
				else
				{
					return $"{FieldType} {{{valueType}}}";
				}
			}
		}
	}

	public FieldNameValuePair(string name, object? value, string? fieldType)
	{
		Name = name;
		Value = value;
		FieldType = fieldType;
	}
}
