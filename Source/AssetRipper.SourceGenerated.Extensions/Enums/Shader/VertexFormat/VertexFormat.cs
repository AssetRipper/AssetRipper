using AssetRipper.SourceGenerated.Extensions.Enums.Shader.ShaderChannel;

namespace AssetRipper.SourceGenerated.Extensions.Enums.Shader.VertexFormat;

/// <summary>
/// Also called VertexAttributeFormat<br/>
/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Graphics/GraphicsEnums.cs"/>
/// </summary>
public enum VertexFormat
{
	Float,
	Float16,
	Color,
	Byte,
	Int,
}

public static class VertexFormatExtensions
{
	/// <summary>
	/// 2019.1 and greater
	/// </summary>
	public static bool VertexFormat2019Relevant(UnityVersion version) => version.GreaterThanOrEquals(2019);
	/// <summary>
	/// 2017.1 and greater
	/// </summary>
	public static bool VertexFormat2017Relevant(UnityVersion version) => version.GreaterThanOrEquals(2017);

	public static byte CalculateStride(this VertexFormat _this, UnityVersion version, int dimention)
	{
		return (byte)(_this.GetSize(version) * dimention);
	}

	public static int GetSize(this VertexFormat _this, UnityVersion version)
	{
		return _this switch
		{
			VertexFormat.Float => 4,
			VertexFormat.Float16 => 2,
			VertexFormat.Color => ShaderChannelExtensions.ShaderChannel5Relevant(version) ? 1 : 4,
			VertexFormat.Byte => 1,
			VertexFormat.Int => 4,
			_ => throw new Exception(_this.ToString()),
		};
	}

	public static byte ToFormat(this VertexFormat _this, UnityVersion version)
	{
		if (VertexFormat2019Relevant(version))
		{
			return (byte)_this.ToVertexFormat2019();
		}
		else if (VertexFormat2017Relevant(version))
		{
			return (byte)_this.ToVertexFormat2017();
		}
		else
		{
			return (byte)_this.ToVertexChannelFormat();
		}
	}

	public static VertexChannelFormat ToVertexChannelFormat(this VertexFormat _this)
	{
		return _this switch
		{
			VertexFormat.Float => VertexChannelFormat.Float,
			VertexFormat.Float16 => VertexChannelFormat.Float16,
			VertexFormat.Color => VertexChannelFormat.Color,
			_ => throw new Exception(_this.ToString()),
		};
	}

	public static VertexFormat2017 ToVertexFormat2017(this VertexFormat _this)
	{
		return _this switch
		{
			VertexFormat.Float => VertexFormat2017.Float,
			VertexFormat.Float16 => VertexFormat2017.Float16,
			VertexFormat.Color => VertexFormat2017.Color,
			VertexFormat.Byte => VertexFormat2017.UInt8,
			VertexFormat.Int => VertexFormat2017.UInt32,
			_ => throw new Exception(_this.ToString()),
		};
	}

	public static VertexFormat2019 ToVertexFormat2019(this VertexFormat _this)
	{
		return _this switch
		{
			VertexFormat.Float => VertexFormat2019.Float,
			VertexFormat.Float16 => VertexFormat2019.Float16,
			VertexFormat.Color or VertexFormat.Byte => VertexFormat2019.UNorm8,
			VertexFormat.Int => VertexFormat2019.UInt32,
			_ => throw new Exception(_this.ToString()),
		};
	}
}
