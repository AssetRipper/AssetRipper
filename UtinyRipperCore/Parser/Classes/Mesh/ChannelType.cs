using System;

namespace UtinyRipper.Classes.Meshes
{
	public enum ChannelType
	{
		Vertex		= 0,
		Normal		= 1,
		Color		= 2,
		UV0			= 3,
		UV1			= 4,
		TangentsOld = 5,
		UV2			= 5,
		UV3			= 6,
		Tangents	= 7,
	}

	public static class ChannelTypeExtensions
	{
		public static byte GetFormat(this ChannelType _this, Version version)
		{
			switch(_this)
			{
				case ChannelType.Vertex:
				case ChannelType.Normal:
					return 0;

				case ChannelType.Color:
					return 2;

				case ChannelType.UV0:
				case ChannelType.UV1:
				case ChannelType.UV3:
					return 0;

				case ChannelType.UV2:
					if (VertexData.IsReadChannels(version))
					{
						// UV2
						return 0;
					}
					else
					{
						// TangentsOld
						return 0;
					}

				case ChannelType.Tangents:
					return 0;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}

		public static byte GetDimention(this ChannelType _this, Version version)
		{
			switch (_this)
			{
				case ChannelType.Vertex:
				case ChannelType.Normal:
					return 3;

				case ChannelType.Color:
					return 4;

				case ChannelType.UV0:
				case ChannelType.UV1:
				case ChannelType.UV3:
					return 2;

				case ChannelType.UV2:
					if(VertexData.IsReadChannels(version))
					{
						// UV2
						return 2;
					}
					else
					{
						// TangentsOld
						return 4;
					}

				case ChannelType.Tangents:
					return 4;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}

		public static byte GetStride(this ChannelType _this, Version version)
		{
			int format = _this.GetFormat(version);
			int dimention = _this.GetDimention(version);
			return ChannelInfo.CalculateStride(format, dimention);
		}
	}
}
