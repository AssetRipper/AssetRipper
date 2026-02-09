namespace AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;

public enum StencilType
{
	Base,
	Front,
	Back,
}

public static class StencilTypeExtensions
{
	public static string ToSuffixString(this StencilType _this) => _this switch
	{
		StencilType.Base => string.Empty,
		StencilType.Front => "Front",
		StencilType.Back => "Back",
		_ => throw new Exception($"Unsupported stencil type {_this}"),
	};
}
