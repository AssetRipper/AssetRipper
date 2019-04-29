using System;

namespace uTinyRipper.Classes.Meshes
{
	/// <summary>
	/// 5.0.0 to 2018.x.x versions
	/// </summary>
	public enum ChannelTypeV5
	{
		Vertex	= 0,
		Normal	= 1,
		Color	= 2,
		UV0		= 3,
		UV1		= 4,
		UV2		= 5,
		UV3		= 6,
		Tangent	= 7,
	}

	public static class ChannelTypeV5Extensions
	{
		public static ChannelType ToChannelType(this ChannelTypeV5 _this)
		{
			switch (_this)
			{
				case ChannelTypeV5.Vertex:
					return ChannelType.Vertex;
				case ChannelTypeV5.Normal:
					return ChannelType.Normal;
				case ChannelTypeV5.Color:
					return ChannelType.Color;
				case ChannelTypeV5.UV0:
					return ChannelType.UV0;
				case ChannelTypeV5.UV1:
					return ChannelType.UV1;
				case ChannelTypeV5.UV2:
					return ChannelType.UV2;
				case ChannelTypeV5.UV3:
					return ChannelType.UV3;
				case ChannelTypeV5.Tangent:
					return ChannelType.Tangent;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}
	}
}
