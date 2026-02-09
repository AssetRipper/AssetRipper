namespace AssetRipper.SourceGenerated.Extensions.Enums.Shader.VertexFormat;

/// <summary>
/// 2019.1 and greater
/// </summary>
public enum VertexFormat2019 : byte
{
	Float = 0,
	Float16 = 1,
	UNorm8 = 2,
	SNorm8 = 3,
	UNorm16 = 4,
	SNorm16 = 5,
	UInt8 = 6,
	SInt8 = 7,
	UInt16 = 8,
	SInt16 = 9,
	UInt32 = 10,
	SInt32 = 11,
}

public static class VertexFormat2019Extension
{
	public static VertexFormat ToVertexFormat(this VertexFormat2019 _this)
	{
		return _this switch
		{
			VertexFormat2019.Float => VertexFormat.Float,
			VertexFormat2019.Float16 => VertexFormat.Float16,
			VertexFormat2019.UNorm8 => VertexFormat.Byte,
			VertexFormat2019.UInt32 => VertexFormat.Int,
			_ => throw new Exception(_this.ToString()),
		};
	}
}
