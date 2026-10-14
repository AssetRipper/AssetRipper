using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;
using AssetRipper.SourceGenerated.Subclasses.SerializedProperty;

namespace AssetRipper.Tests;

internal class SerializedPropertyTests
{
	[TestCase(SerializedPropertyType.Color, "Color")]
	[TestCase(SerializedPropertyType.Vector, "Vector")]
	[TestCase(SerializedPropertyType.Float, "Float")]
	// "Int" would declare a float property, so materials would ignore their integer values.
	[TestCase(SerializedPropertyType.Int, "Integer")]
	public void TypeStringMatchesShaderLab(SerializedPropertyType type, string expected)
	{
		ISerializedProperty property = SerializedProperty.Create(new UnityVersion(6000, 2));
		property.Type = (int)type;

		Assert.That(property.GetTypeString(), Is.EqualTo(expected));
	}
}
