using AssetRipper.SourceGenerated.Extensions.Enums.Shader.ShaderChannel;
using AssetRipper.SourceGenerated.Subclasses.StreamInfo;

namespace AssetRipper.SourceGenerated.Extensions;

public static class StreamInfoExtensions
{
	public static uint GetStride(this IStreamInfo streamInfo)
	{
		return streamInfo.Has_Stride_UInt32() ? streamInfo.Stride_UInt32 : streamInfo.Stride_Byte;
	}

	public static void SetStride(this IStreamInfo streamInfo, uint stride)
	{
		if (streamInfo.Has_Stride_Byte())
		{
			streamInfo.Stride_Byte = (byte)stride;
		}
		else
		{
			streamInfo.Stride_UInt32 = stride;
		}
	}

	public static void SetValues(this IStreamInfo streamInfo, uint mask, uint offset, uint stride)
	{
		streamInfo.ChannelMask = mask;
		streamInfo.Offset = offset;
		streamInfo.SetStride(stride);
		streamInfo.Align = 0;
		streamInfo.DividerOp = 0;
		streamInfo.Frequency = 0;
	}

	public static bool IsMatch(this IStreamInfo streamInfo, ShaderChannel4 channel)
	{
		return (streamInfo.ChannelMask & 1 << (int)channel) != 0;
	}
}
