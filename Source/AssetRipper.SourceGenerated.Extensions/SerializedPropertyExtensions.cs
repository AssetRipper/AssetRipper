using AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;
using AssetRipper.SourceGenerated.Subclasses.SerializedProperty;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SerializedPropertyExtensions
{
	public static SerializedPropertyType GetType_(this ISerializedProperty property)
	{
		return (SerializedPropertyType)property.Type;
	}

	public static SerializedPropertyFlag GetFlags(this ISerializedProperty property)
	{
		return (SerializedPropertyFlag)property.Flags;
	}

	public static string[] GetAttributes(this ISerializedProperty property)
	{
		List<string> attributes = new(property.Attributes.Count + 6);
		foreach (Utf8String attribute in property.Attributes)
		{
			attributes.Add(attribute.ToString());
		}
		SerializedPropertyFlag flags = property.GetFlags();
		if (flags.IsHideInInspector())
		{
			attributes.Add("HideInInspector");
		}
		if (flags.IsPerRendererData())
		{
			attributes.Add("PerRendererData");
		}
		if (flags.IsNoScaleOffset())
		{
			attributes.Add("NoScaleOffset");
		}
		if (flags.IsNormal())
		{
			attributes.Add("Normal");
		}
		if (flags.IsHDR())
		{
			attributes.Add("HDR");
		}
		if (flags.IsGamma())
		{
			attributes.Add("Gamma");
		}
		return attributes.ToArray();
	}

	public static string GetTypeString(this ISerializedProperty property) => property.GetType_() switch
	{
		SerializedPropertyType.Color => "Color",
		SerializedPropertyType.Vector => "Vector",
		SerializedPropertyType.Float => "Float",
		SerializedPropertyType.Range => $"{"Range"}({property.DefValue_1_.ToStringInvariant()}, {property.DefValue_2_.ToStringInvariant()})",
		SerializedPropertyType.Texture => property.DefTexture.TexDim switch
		{
			1 => "any",
			2 => "2D",
			3 => "3D",
			4 => "Cube",
			5 => "2DArray",
			6 => "CubeArray",
			_ => throw new NotSupportedException("Texture dimension isn't supported"),
		},
		SerializedPropertyType.Int => "Int",
		_ => throw new NotSupportedException($"Serialized property type {property.Type} isn't supported"),
	};

	public static string GetDefaultValue(this ISerializedProperty property)
	{
		switch (property.GetType_())
		{
			case SerializedPropertyType.Color:
			case SerializedPropertyType.Vector:
				string v0 = property.DefValue_0_.ToStringInvariant();
				string v1 = property.DefValue_1_.ToStringInvariant();
				string v2 = property.DefValue_2_.ToStringInvariant();
				string v3 = property.DefValue_3_.ToStringInvariant();
				return $"({v0},{v1},{v2},{v3})";

			case SerializedPropertyType.Float:
			case SerializedPropertyType.Range:
			case SerializedPropertyType.Int:
				return property.DefValue_0_.ToStringInvariant();

			case SerializedPropertyType.Texture:
				return $$"""
					"{{property.DefTexture.DefaultName}}" {}
					""";

			default:
				throw new NotSupportedException($"Serialized property type {property.Type} isn't supported");
		}
	}
}
