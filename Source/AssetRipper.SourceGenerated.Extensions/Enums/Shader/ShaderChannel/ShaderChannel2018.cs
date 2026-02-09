namespace AssetRipper.SourceGenerated.Extensions.Enums.Shader.ShaderChannel;

/// <summary>
/// 2018.1 and greater version
/// </summary>
public enum ShaderChannel2018
{
	Vertex = 0,
	Normal = 1,
	Tangent = 2,
	Color = 3,
	UV0 = 4,
	UV1 = 5,
	UV2 = 6,
	UV3 = 7,
	UV4 = 8,
	UV5 = 9,
	UV6 = 10,
	UV7 = 11,
	SkinWeight = 12,
	SkinBoneIndex = 13,
}

public static class ShaderChannelV2018Extensions
{
	public static ShaderChannel ToShaderChannel(this ShaderChannel2018 _this)
	{
		return _this switch
		{
			ShaderChannel2018.Vertex => ShaderChannel.Vertex,
			ShaderChannel2018.Normal => ShaderChannel.Normal,
			ShaderChannel2018.Tangent => ShaderChannel.Tangent,
			ShaderChannel2018.Color => ShaderChannel.Color,
			ShaderChannel2018.UV0 => ShaderChannel.UV0,
			ShaderChannel2018.UV1 => ShaderChannel.UV1,
			ShaderChannel2018.UV2 => ShaderChannel.UV2,
			ShaderChannel2018.UV3 => ShaderChannel.UV3,
			ShaderChannel2018.UV4 => ShaderChannel.UV4,
			ShaderChannel2018.UV5 => ShaderChannel.UV5,
			ShaderChannel2018.UV6 => ShaderChannel.UV6,
			ShaderChannel2018.UV7 => ShaderChannel.UV7,
			ShaderChannel2018.SkinWeight => ShaderChannel.SkinWeight,
			ShaderChannel2018.SkinBoneIndex => ShaderChannel.SkinBoneIndex,
			_ => throw new Exception($"Unsupported channel type {_this}"),
		};
	}
}
