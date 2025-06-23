using AssetRipper.SourceGenerated.Extensions.Enums.Shader.VertexFormat;

namespace AssetRipper.SourceGenerated.Extensions.Enums.Shader.ShaderChannel;

/// <summary>
/// Changed several times. Also called VertexAttribute<br/>
/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Graphics/GraphicsEnums.cs"/>
/// </summary>
public enum ShaderChannel
{
	/// <summary>
	/// Also called Position
	/// </summary>
	Vertex,
	Normal,
	Tangent,
	/// <summary>
	/// Vertex Color
	/// </summary>
	Color,
	UV0,
	UV1,
	UV2,
	UV3,
	UV4,
	UV5,
	UV6,
	UV7,
	/// <summary>
	/// Also called BlendWeight
	/// </summary>
	SkinWeight,
	/// <summary>
	/// Also called BlendIndices
	/// </summary>
	SkinBoneIndex,
}

public static class ShaderChannelExtensions
{
	/// <summary>
	/// 2018.1 and greater
	/// </summary>
	public static bool ShaderChannel2018Relevant(UnityVersion version) => version.GreaterThanOrEquals(2018);
	/// <summary>
	/// 5.0.0 and greater
	/// </summary>
	public static bool ShaderChannel5Relevant(UnityVersion version) => version.GreaterThanOrEquals(5);

	public static int GetChannelCount(UnityVersion version)
	{
		if (ShaderChannel2018Relevant(version))
		{
			return 14;
		}
		else if (ShaderChannel5Relevant(version))
		{
			return 8;
		}
		else
		{
			return 6;
		}
	}

	public static VertexFormat.VertexFormat GetVertexFormat(this ShaderChannel _this, UnityVersion version)
	{
		switch (_this)
		{
			case ShaderChannel.Vertex:
				return VertexFormat.VertexFormat.Float;
			case ShaderChannel.Normal:
				return VertexFormat.VertexFormat.Float;
			case ShaderChannel.Tangent:
				return VertexFormat.VertexFormat.Float;
			case ShaderChannel.Color:
				return VertexFormatExtensions.VertexFormat2019Relevant(version) ? VertexFormat.VertexFormat.Byte : VertexFormat.VertexFormat.Color;

			case ShaderChannel.UV0:
			case ShaderChannel.UV1:
			case ShaderChannel.UV2:
			case ShaderChannel.UV3:
			case ShaderChannel.UV4:
			case ShaderChannel.UV5:
			case ShaderChannel.UV6:
			case ShaderChannel.UV7:
				return VertexFormat.VertexFormat.Float;

			case ShaderChannel.SkinWeight:
				return VertexFormat.VertexFormat.Float;
			case ShaderChannel.SkinBoneIndex:
				return VertexFormat.VertexFormat.Int;

			default:
				throw new Exception($"Unsupported channel type {_this}");
		}
	}

	public static byte GetDimention(this ShaderChannel _this, UnityVersion version)
	{
		switch (_this)
		{
			case ShaderChannel.Vertex:
				return 3;
			case ShaderChannel.Normal:
				return 3;
			case ShaderChannel.Tangent:
				return 4;
			case ShaderChannel.Color:
				return ShaderChannel5Relevant(version) ? (byte)4 : (byte)1;

			case ShaderChannel.UV0:
			case ShaderChannel.UV1:
			case ShaderChannel.UV2:
			case ShaderChannel.UV3:
			case ShaderChannel.UV4:
			case ShaderChannel.UV5:
			case ShaderChannel.UV6:
			case ShaderChannel.UV7:
				return 2;

			case ShaderChannel.SkinWeight:
			case ShaderChannel.SkinBoneIndex:
				throw new Exception($"Skin's dimention is varying");

			default:
				throw new Exception($"Unsupported channel type {_this}");
		}
	}

	public static byte GetStride(this ShaderChannel _this, UnityVersion version)
	{
		VertexFormat.VertexFormat format = _this.GetVertexFormat(version);
		int dimention = _this.GetDimention(version);
		return format.CalculateStride(version, dimention);
	}

	public static bool HasChannel(this ShaderChannel _this, UnityVersion version)
	{
		if (ShaderChannel2018Relevant(version))
		{
			return true;
		}
		else if (ShaderChannel5Relevant(version))
		{
			return _this <= ShaderChannel.UV3;
		}
		else
		{
			return _this <= ShaderChannel.UV1;
		}
	}

	public static int ToChannel(this ShaderChannel _this, UnityVersion version)
	{
		if (ShaderChannel2018Relevant(version))
		{
			return (int)_this.ToShaderChannel2018();
		}
		else if (ShaderChannel5Relevant(version))
		{
			return (int)_this.ToShaderChannel5();
		}
		else
		{
			return (int)_this.ToShaderChannel4();
		}
	}

	public static ShaderChannel4 ToShaderChannel4(this ShaderChannel _this)
	{
		return _this switch
		{
			ShaderChannel.Vertex => ShaderChannel4.Vertex,
			ShaderChannel.Normal => ShaderChannel4.Normal,
			ShaderChannel.Color => ShaderChannel4.Color,
			ShaderChannel.UV0 => ShaderChannel4.UV0,
			ShaderChannel.UV1 => ShaderChannel4.UV1,
			ShaderChannel.Tangent => ShaderChannel4.Tangent,
			_ => throw new Exception($"Unsupported channel type {_this}"),
		};
	}

	public static ShaderChannel5 ToShaderChannel5(this ShaderChannel _this)
	{
		return _this switch
		{
			ShaderChannel.Vertex => ShaderChannel5.Vertex,
			ShaderChannel.Normal => ShaderChannel5.Normal,
			ShaderChannel.Color => ShaderChannel5.Color,
			ShaderChannel.UV0 => ShaderChannel5.UV0,
			ShaderChannel.UV1 => ShaderChannel5.UV1,
			ShaderChannel.UV2 => ShaderChannel5.UV2,
			ShaderChannel.UV3 => ShaderChannel5.UV3,
			ShaderChannel.Tangent => ShaderChannel5.Tangent,
			_ => throw new Exception($"Unsupported channel type {_this}"),
		};
	}

	public static ShaderChannel2018 ToShaderChannel2018(this ShaderChannel _this)
	{
		return _this switch
		{
			ShaderChannel.Vertex => ShaderChannel2018.Vertex,
			ShaderChannel.Normal => ShaderChannel2018.Normal,
			ShaderChannel.Tangent => ShaderChannel2018.Tangent,
			ShaderChannel.Color => ShaderChannel2018.Color,
			ShaderChannel.UV0 => ShaderChannel2018.UV0,
			ShaderChannel.UV1 => ShaderChannel2018.UV1,
			ShaderChannel.UV2 => ShaderChannel2018.UV2,
			ShaderChannel.UV3 => ShaderChannel2018.UV3,
			ShaderChannel.UV4 => ShaderChannel2018.UV4,
			ShaderChannel.UV5 => ShaderChannel2018.UV5,
			ShaderChannel.UV6 => ShaderChannel2018.UV6,
			ShaderChannel.UV7 => ShaderChannel2018.UV7,
			ShaderChannel.SkinWeight => ShaderChannel2018.SkinWeight,
			ShaderChannel.SkinBoneIndex => ShaderChannel2018.SkinBoneIndex,
			_ => throw new Exception($"Unsupported channel type {_this}"),
		};
	}

	public static string ToSemantic(this ShaderChannel _this) => _this switch
	{
		ShaderChannel.Vertex => "POSITION",
		ShaderChannel.Normal => "NORMAL",
		ShaderChannel.Tangent => "TANGENT",
		ShaderChannel.Color => "COLOR",
		ShaderChannel.UV0 => "TEXCOORD0",
		ShaderChannel.UV1 => "TEXCOORD1",
		ShaderChannel.UV2 => "TEXCOORD2",
		ShaderChannel.UV3 => "TEXCOORD3",
		ShaderChannel.UV4 => "TEXCOORD4",
		ShaderChannel.UV5 => "TEXCOORD5",
		ShaderChannel.UV6 => "TEXCOORD6",
		ShaderChannel.UV7 => "TEXCOORD7",
		ShaderChannel.SkinWeight => "BLENDWEIGHT",
		ShaderChannel.SkinBoneIndex => "BLENDINDICES",
		_ => throw new Exception($"Unsupported channel type {_this}"),
	};
}
