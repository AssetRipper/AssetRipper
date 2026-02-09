namespace AssetRipper.SourceGenerated.Extensions.Enums.Shader;

public enum ShaderType
{
	None = 0,
	Vertex = 1,
	Fragment = 2,
	Geometry = 3,
	Hull = 4,
	Domain = 5,
	/// <summary>
	/// 2019.3 and greater
	/// </summary>
	RayTracing = 6,

	TypeCount,
}

public static class ShaderTypeExtensions
{
	public static string ToProgramTypeString(this ShaderType _this)
	{
		return _this switch
		{
			ShaderType.Vertex => "vp",
			ShaderType.Fragment => "fp",
			ShaderType.Geometry => "gp",
			ShaderType.Hull => "hp",
			ShaderType.Domain => "dp",
			ShaderType.RayTracing => "rtp",
			_ => throw new NotSupportedException($"ShaderType {_this} isn't supported"),
		};
	}

	public static int ToProgramMask(this ShaderType _this)
	{
		return 1 << (int)_this;
	}
}
