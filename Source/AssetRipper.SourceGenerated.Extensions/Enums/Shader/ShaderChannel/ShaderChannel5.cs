namespace AssetRipper.SourceGenerated.Extensions.Enums.Shader.ShaderChannel;

/// <summary>
/// 5.0.0 to 2017.x versions
/// </summary>
public enum ShaderChannel5
{
	Vertex = 0,
	Normal = 1,
	Color = 2,
	UV0 = 3,
	UV1 = 4,
	UV2 = 5,
	UV3 = 6,
	Tangent = 7,
}

public static class ShaderChannelV5Extensions
{
	public static ShaderChannel ToShaderChannel(this ShaderChannel5 _this)
	{
		return _this switch
		{
			ShaderChannel5.Vertex => ShaderChannel.Vertex,
			ShaderChannel5.Normal => ShaderChannel.Normal,
			ShaderChannel5.Color => ShaderChannel.Color,
			ShaderChannel5.UV0 => ShaderChannel.UV0,
			ShaderChannel5.UV1 => ShaderChannel.UV1,
			ShaderChannel5.UV2 => ShaderChannel.UV2,
			ShaderChannel5.UV3 => ShaderChannel.UV3,
			ShaderChannel5.Tangent => ShaderChannel.Tangent,
			_ => throw new Exception($"Unsupported channel type {_this}"),
		};
	}
}
