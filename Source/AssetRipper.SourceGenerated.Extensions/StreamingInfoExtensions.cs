using AssetRipper.Assets.Collections;
using AssetRipper.SourceGenerated.Subclasses.StreamingInfo;

namespace AssetRipper.SourceGenerated.Extensions;

public static class StreamingInfoExtensions
{
	public static bool IsSet(this IStreamingInfo streamingInfo) => streamingInfo.Path.Data.Length > 0;

	public static bool CheckIntegrity(this IStreamingInfo streamingInfo, AssetCollection file)
	{
		return StreamedResourceExtensions.CheckIntegrity(streamingInfo.Path, streamingInfo.GetOffset(), streamingInfo.Size, file);
	}

	public static byte[] GetContent(this IStreamingInfo streamingInfo, AssetCollection file)
	{
		return StreamedResourceExtensions.GetContent(streamingInfo.Path, streamingInfo.GetOffset(), streamingInfo.Size, file) ?? [];
	}

	public static ulong GetOffset(this IStreamingInfo streamingInfo)
	{
		return streamingInfo.Has_Offset_UInt64() ? streamingInfo.Offset_UInt64 : streamingInfo.Offset_UInt32;
	}

	public static void SetOffset(this IStreamingInfo streamingInfo, ulong value)
	{
		if (streamingInfo.Has_Offset_UInt64())
		{
			streamingInfo.Offset_UInt64 = value;
		}
		else
		{
			streamingInfo.Offset_UInt32 = (uint)value;
		}
	}

	public static void CopyValues(this IStreamingInfo destination, IStreamingInfo source)
	{
		destination.SetOffset(source.GetOffset());
		destination.Size = source.Size;
		destination.Path = source.Path;
	}

	public static void ClearValues(this IStreamingInfo streamingInfo)
	{
		streamingInfo.Offset_UInt32 = default;
		streamingInfo.Offset_UInt64 = default;
		streamingInfo.Path = Utf8String.Empty;
		streamingInfo.Size = default;
	}

	public static void GetValues(this IStreamingInfo streamingInfo, out Utf8String path, out ulong offset, out uint size)
	{
		path = streamingInfo.Path;
		offset = streamingInfo.GetOffset();
		size = streamingInfo.Size;
	}

	public static void SetValues(this IStreamingInfo streamingInfo, Utf8String path, ulong offset, uint size)
	{
		streamingInfo.Path = path;
		streamingInfo.SetOffset(offset);
		streamingInfo.Size = size;
	}
}
