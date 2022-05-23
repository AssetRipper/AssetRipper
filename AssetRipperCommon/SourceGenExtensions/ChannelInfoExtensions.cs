using AssetRipper.Core.Classes.Shader.Enums.VertexFormat;
using AssetRipper.SourceGenerated.Subclasses.ChannelInfo;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class ChannelInfoExtensions
	{
		public static bool IsSet(this IChannelInfo channelInfo) => channelInfo.Dimension > 0;

		/// <summary>
		/// Data dimention: Vector3, Vector2, Vector1
		/// </summary>
		public static byte GetDataDimension(this IChannelInfo channelInfo)
		{
			return (byte)(channelInfo.Dimension & 0b00001111);
		}

		/// <summary>
		/// Data dimention: Vector3, Vector2, Vector1
		/// </summary>
		public static void SetDataDimension(this IChannelInfo channelInfo, byte value)
		{
			channelInfo.Dimension = (byte)((channelInfo.Dimension & 0b11110000) | (value & 0b00001111));
		}

		public static byte GetStride(this IChannelInfo channelInfo, UnityVersion version)
		{
			return channelInfo.GetVertexFormat(version).CalculateStride(version, channelInfo.GetDataDimension());
		}

		public static VertexFormat GetVertexFormat(this IChannelInfo channelInfo, UnityVersion version)
		{
			if (VertexFormatExtensions.VertexFormat2019Relevant(version))
			{
				return ((VertexFormat2019)channelInfo.Format).ToVertexFormat();
			}
			else if (VertexFormatExtensions.VertexFormat2017Relevant(version))
			{
				return ((VertexFormat2017)channelInfo.Format).ToVertexFormat();
			}
			else
			{
				return ((VertexChannelFormat)channelInfo.Format).ToVertexFormat();
			}
		}
	}
}
