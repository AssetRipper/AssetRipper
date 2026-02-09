namespace AssetRipper.SourceGenerated.Extensions.Enums.Shader.VertexFormat;

/// <summary>
/// Less than 2017.1
/// </summary>
public enum VertexChannelFormat : byte
{
	Float = 0,
	Float16 = 1,
	Color = 2,
	Byte = 3,
	UInt = 4,
}

public static class VertexChannelFormatV4Extension
{
	public static VertexFormat ToVertexFormat(this VertexChannelFormat _this)
	{
		return _this switch
		{
			VertexChannelFormat.Float => VertexFormat.Float,
			VertexChannelFormat.Float16 => VertexFormat.Float16,
			VertexChannelFormat.Color => VertexFormat.Color,
			_ => throw new Exception(_this.ToString()),
		};
	}
}
