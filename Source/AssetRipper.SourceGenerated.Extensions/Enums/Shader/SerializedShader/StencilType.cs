namespace AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader
{
	public enum StencilType
	{
		Base,
		Front,
		Back,
	}

	public static class StencilTypeExtensions
	{
		public static string ToSuffixString(this StencilType _this)
		{
			if (_this == StencilType.Base)
			{
				return string.Empty;
			}
			return _this.ToString();
		}
	}
}
