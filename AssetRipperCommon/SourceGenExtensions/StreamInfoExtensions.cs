using AssetRipper.SourceGenerated.Subclasses.StreamInfo;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class StreamInfoExtensions
	{
		public static uint GetStride(this IStreamInfo streamInfo)
		{
			return streamInfo.Has_Stride_UInt32() ? streamInfo.Stride_UInt32 : streamInfo.Stride_Byte;
		}
	}
}
