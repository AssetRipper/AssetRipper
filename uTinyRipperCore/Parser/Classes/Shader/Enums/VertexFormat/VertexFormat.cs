using System;

namespace uTinyRipper.Classes.Shaders
{
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
		public static bool VertexFormat2019Relevant(Version version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool VertexFormat2017Relevant(Version version) => version.IsGreaterEqual(2017);

		public static byte CalculateStride(this VertexFormat _this, Version version, int dimention)
		{
			return (byte)(_this.GetSize(version) * dimention);
		}

		public static int GetSize(this VertexFormat _this, Version version)
		{
			switch (_this)
			{
				case VertexFormat.Float:
					return 4;
				case VertexFormat.Float16:
					return 2;
				case VertexFormat.Color:
					return ShaderChannelExtensions.ShaderChannel5Relevant(version) ? 1 : 4;
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
			switch (_this)
			{
				case VertexFormat.Float:
					return VertexChannelFormat.Float;
				case VertexFormat.Float16:
					return VertexChannelFormat.Float16;
				case VertexFormat.Color:
					return VertexChannelFormat.Color;

				default:
					throw new Exception(_this.ToString());
			}
		}

		public static VertexFormat2017 ToVertexFormat2017(this VertexFormat _this)
		{
			switch (_this)
			{
				case VertexFormat.Float:
					return VertexFormat2017.Float;
				case VertexFormat.Float16:
					return VertexFormat2017.Float16;
				case VertexFormat.Color:
					return VertexFormat2017.Color;
				case VertexFormat.Byte:
					return VertexFormat2017.UInt8;
				case VertexFormat.Int:
					return VertexFormat2017.UInt32;

				default:
					throw new Exception(_this.ToString());
			}
		}

		public static VertexFormat2019 ToVertexFormat2019(this VertexFormat _this)
		{
			switch (_this)
			{
				case VertexFormat.Float:
					return VertexFormat2019.Float;
				case VertexFormat.Float16:
					return VertexFormat2019.Float16;
				case VertexFormat.Color:
				case VertexFormat.Byte:
					return VertexFormat2019.UNorm8;
				case VertexFormat.Int:
					return VertexFormat2019.UInt32;

				default:
					throw new Exception(_this.ToString());
			}
		}
	}
}
