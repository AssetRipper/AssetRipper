namespace AssetRipper.SourceGenerated.Extensions.Enums.Shader.ShaderChannel;

/// <summary>
/// Less than 5.0.0 version
/// </summary>
public enum ShaderChannel4
{
	Vertex = 0,
	Normal = 1,
	Color = 2,
	UV0 = 3,
	UV1 = 4,
	Tangent = 5,
}

public static class ShaderChannelV4Extensions
{
	public static ShaderChannel ToShaderChannel(this ShaderChannel4 _this)
	{
		return _this switch
		{
			ShaderChannel4.Vertex => ShaderChannel.Vertex,
			ShaderChannel4.Normal => ShaderChannel.Normal,
			ShaderChannel4.Color => ShaderChannel.Color,
			ShaderChannel4.UV0 => ShaderChannel.UV0,
			ShaderChannel4.UV1 => ShaderChannel.UV1,
			ShaderChannel4.Tangent => ShaderChannel.Tangent,
			_ => throw new Exception($"Unsupported channel type {_this}"),
		};
	}
}
