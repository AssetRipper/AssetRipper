using AssetRipper.Core.Extensions;
using AssetRipper.Core.Parser.Files.ResourceFiles;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.SourceGenerated.Subclasses.StreamingInfo;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class StreamingInfoExtensions
	{
		public static bool IsSet(this IStreamingInfo streamingInfo) => !streamingInfo.Path.Data.IsNullOrEmpty();

		public static bool CheckIntegrity(this IStreamingInfo streamingInfo, ISerializedFile file)
		{
			if (!streamingInfo.IsSet())
			{
				return true;
			}
			return file.Collection.FindResourceFile(streamingInfo.Path.String) != null;
		}

		public static byte[] GetContent(this IStreamingInfo streamingInfo, ISerializedFile file)
		{
			IResourceFile? res = file.Collection.FindResourceFile(streamingInfo.Path.String);
			if (res == null)
			{
				return Array.Empty<byte>();
			}

			byte[] data = new byte[streamingInfo.Size];
			res.Stream.Position = (long)streamingInfo.GetOffset();
			res.Stream.ReadBuffer(data, 0, data.Length);
			return data;
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
			destination.Path.String = source.Path.String;
		}

		public static void ClearValues(this IStreamingInfo streamingInfo)
		{
			streamingInfo.Offset_UInt32 = default;
			streamingInfo.Offset_UInt64 = default;
			streamingInfo.Path.Data = Array.Empty<byte>();
			streamingInfo.Size = default;
		}
	}
}
