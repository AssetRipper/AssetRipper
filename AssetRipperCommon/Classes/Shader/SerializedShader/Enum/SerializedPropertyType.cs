namespace AssetRipper.Core.Classes.Shader.SerializedShader.Enum
{
	/// <summary>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Shaders/ShaderProperties.cs"/>
	/// </summary>
	public enum SerializedPropertyType
	{
		Color = 0,
		Vector = 1,
		Int = 2,
		Float = 2,
		Range = 3,
		_2D = 4,
		_2DArray = 4,
		_3D = 4,
		Cube = 4,
		CubeArray = 4,
	}

	public static class SerializedPropertyTypeExtensions
	{
		public static bool IsTexture(this SerializedPropertyType _this)
		{
			return _this == SerializedPropertyType._2D || _this == SerializedPropertyType._3D || _this == SerializedPropertyType.Cube;
		}
	}
}
