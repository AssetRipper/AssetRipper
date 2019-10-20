using System;

namespace uTinyRipper.Classes.Meshes
{
	/// <summary>
	/// 2019.1 and greater
	/// </summary>
	public enum VertexFormatV2019 : byte
	{
		Float		= 0,
		Float16		= 1,
		UNorm8		= 2,
		SNorm8		= 3,
		UNorm16		= 4,
		SNorm16		= 5,
		UInt8		= 6,
		SInt8		= 7,
		UInt16		= 8,
		SInt16		= 9,
		UInt32		= 10,
		SInt32		= 11,
	}

	public static class VertexFormat2019Extension
	{
		public static VertexFormat ToVertexFormat(this VertexFormatV2019 _this)
		{
			switch (_this)
			{
				case VertexFormatV2019.Float:
					return VertexFormat.Float;
				case VertexFormatV2019.Float16:
					return VertexFormat.Float16;
				case VertexFormatV2019.UNorm8:
					return VertexFormat.Byte;
				case VertexFormatV2019.UInt32:
					return VertexFormat.Int;

				default:
					throw new Exception(_this.ToString());
			}
		}
	}
}
