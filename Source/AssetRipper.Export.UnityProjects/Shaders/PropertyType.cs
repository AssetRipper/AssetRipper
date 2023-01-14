using AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;

namespace AssetRipper.Export.UnityProjects.Shaders;

public enum PropertyType
{
	Color = 0,
	Vector = 1,
	Single = 2,
	Range = 3,
	Texture = 4,
}

public static class PropertyTypeExtensions
{
	public static bool IsMatch(this PropertyType _this, SerializedPropertyType type) => (int)_this == (int)type;
	public static bool IsMatch(this PropertyType _this, int type) => (int)_this == type;
}
