using System;

namespace uTinyRipper.Classes.Meshes
{
	public enum VertexFormat
	{
		Float,
		Float16,
		Color,
		Byte,
		Int,
	}

	public static class VertexFormatExtension
	{
		public static byte CalculateStride(this VertexFormat _this, int dimention)
		{
			return (byte)(_this.GetSize() * dimention);
		}

		public static int GetSize(this VertexFormat _this)
		{
			switch (_this)
			{
				case VertexFormat.Float:
					return 4;
				case VertexFormat.Float16:
					return 2;
				case VertexFormat.Color:
					return 4;
				case VertexFormat.Byte:
					return 1;
				case VertexFormat.Int:
					return 4;

				default:
					throw new Exception(_this.ToString());
			}
		}

		public static byte ToFormat(this VertexFormat _this, Version version)
		{
			if (version.IsGreaterEqual(2019))
			{
				return (byte)_this.ToVertexFormatV2019();
			}
			else if (version.IsGreaterEqual(2017))
			{
				return (byte)_this.ToVertexFormatV2017();
			}
			else
			{
				return (byte)_this.ToVertexChannelFormat();
			}
		}

		public static VertexChannelFormat ToVertexChannelFormat(this VertexFormat _this)
		{
			switch (_this)
			{
				case VertexFormat.Float:
					return VertexChannelFormat.Float;
				case VertexFormat.Float16:
					return VertexChannelFormat.Float16;
				case VertexFormat.Color:
					return VertexChannelFormat.Color;
				case VertexFormat.Byte:
					return VertexChannelFormat.Byte;
				case VertexFormat.Int:
					return VertexChannelFormat.UInt;

				default:
					throw new Exception(_this.ToString());
			}
		}

		public static VertexFormatV2017 ToVertexFormatV2017(this VertexFormat _this)
		{
			switch (_this)
			{
				case VertexFormat.Float:
					return VertexFormatV2017.Float;
				case VertexFormat.Float16:
					return VertexFormatV2017.Float16;
				case VertexFormat.Byte:
					return VertexFormatV2017.UNorm8;
				case VertexFormat.Int:
					return VertexFormatV2017.UInt32;

				default:
					throw new Exception(_this.ToString());
			}
		}

		public static VertexFormatV2019 ToVertexFormatV2019(this VertexFormat _this)
		{
			switch (_this)
			{
				case VertexFormat.Float:
					return VertexFormatV2019.Float;
				case VertexFormat.Float16:
					return VertexFormatV2019.Float16;
				case VertexFormat.Byte:
					return VertexFormatV2019.UNorm8;
				case VertexFormat.Int:
					return VertexFormatV2019.UInt32;

				default:
					throw new Exception(_this.ToString());
			}
		}
	}
}
