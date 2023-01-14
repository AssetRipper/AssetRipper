namespace AssetRipper.IO.Files.SourceGenerator.Json;

internal sealed class FieldDefinition
{
	/// <summary>
	/// The name of this field's type.
	/// </summary>
	public string TypeName { get; set; } = string.Empty;
	/// <summary>
	/// The name of this field.
	/// </summary>
	/// <remarks>
	/// Null is assumed to be filler bytes. In that case, <see cref="TypeName"/> must be a primitive type of predefined size.
	/// </remarks>
	public string? FieldName { get; set; }
	/// <summary>
	/// Special flags for this field. For example:
	/// <list type="bullet">
	/// <item>enum</item>
	/// </list>
	/// </summary>
	public SpecialDetails? Special { get; set; }

	public override string ToString() => $"{TypeName} {FieldName}";

	public bool TypeIsEnum([NotNullWhen(true)] out string? enumName)
	{
		if (Special?.Identifier == "enum")
		{
			enumName = Special.Parameter;
			return enumName is not null;
		}
		else
		{
			enumName = null;
			return false;
		}
	}

	public bool TypeIsString(out StringSerialization type)
	{
		if (TypeName == "string")
		{
			type = Special?.Identifier switch
			{
				"NullTerminated" => StringSerialization.NullTerminated,
				_ => StringSerialization.Unknown,
			};
			return true;
		}
		else
		{
			type = default;
			return false;
		}
	}

	public bool TypeIsByteAlignment4() => TypeName == "align4";

	public bool TypeIsByteAlignment16() => TypeName == "align16";
}
