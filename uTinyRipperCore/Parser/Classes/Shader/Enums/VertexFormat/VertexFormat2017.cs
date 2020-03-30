using System;

namespace uTinyRipper.Classes.Shaders
{
	/// <summary>
	/// 2017.1 to 2018.x.x
	/// </summary>
	public enum VertexFormat2017 : byte
	{
		Float		= 0,
		Float16		= 1,
		Color		= 2,
		UNorm8		= 3,
		SNorm8		= 4,
		UNorm16		= 5,
		SNorm16		= 6,
		UInt8		= 7,
		SInt8		= 8,
		UInt16		= 9,
		SInt16		= 10,
		UInt32		= 11,
		SInt32		= 12,
	}

	public static class VertexFormatV5Extension
	{
		public static VertexFormat ToVertexFormat(this VertexFormat2017 _this)
		{
			switch (_this)
			{
				case VertexFormat2017.Float:
					return VertexFormat.Float;
				case VertexFormat2017.Float16:
					return VertexFormat.Float16;
				case VertexFormat2017.Color:
					return VertexFormat.Color;
				case VertexFormat2017.UNorm8:
					return VertexFormat.Byte;
				case VertexFormat2017.UInt32:
					return VertexFormat.Int;

				default:
					throw new Exception(_this.ToString());
			}
		}
	}
}
