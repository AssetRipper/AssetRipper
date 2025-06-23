namespace AssetRipper.SourceGenerated.Extensions.Enums.Shader.VertexFormat;

/// <summary>
/// 2017.1 to 2018.x.x
/// </summary>
public enum VertexFormat2017 : byte
{
	Float = 0,
	Float16 = 1,
	Color = 2,
	UNorm8 = 3,
	SNorm8 = 4,
	UNorm16 = 5,
	SNorm16 = 6,
	UInt8 = 7,
	SInt8 = 8,
	UInt16 = 9,
	SInt16 = 10,
	UInt32 = 11,
	SInt32 = 12,
}

public static class VertexFormatV5Extension
{
	public static VertexFormat ToVertexFormat(this VertexFormat2017 _this)
	{
		return _this switch
		{
			VertexFormat2017.Float => VertexFormat.Float,
			VertexFormat2017.Float16 => VertexFormat.Float16,
			VertexFormat2017.Color => VertexFormat.Color,
			VertexFormat2017.UNorm8 => VertexFormat.Byte,
			VertexFormat2017.UInt32 => VertexFormat.Int,
			_ => throw new Exception(_this.ToString()),
		};
	}
}
