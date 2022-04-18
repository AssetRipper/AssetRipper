using AssetRipper.Core.Extensions;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.ResourceFiles;
using AssetRipper.Core.Parser.Files.SerializedFiles;

namespace AssetRipper.Core.Classes.Misc
{
	public interface IStreamingInfo : IUnityAssetBase
	{
		/// <summary>
		/// uint on versions &lt; 2020
		/// </summary>
		long Offset { get; set; }
		uint Size { get; set; }
		string Path { get; set; }
	}

	public static class StreamingInfoExtensions
	{
		public static bool IsSet(this IStreamingInfo streamingInfo) => !string.IsNullOrEmpty(streamingInfo.Path);

		public static bool CheckIntegrity(this IStreamingInfo streamingInfo, ISerializedFile file)
		{
			if (!streamingInfo.IsSet())
			{
				return true;
			}
			return file.Collection.FindResourceFile(streamingInfo.Path) != null;
		}

		public static byte[] GetContent(this IStreamingInfo streamingInfo, ISerializedFile file)
		{
			IResourceFile res = file.Collection.FindResourceFile(streamingInfo.Path);
			if (res == null)
			{
				return null;
			}

			byte[] data = new byte[streamingInfo.Size];
			res.Stream.Position = streamingInfo.Offset;
			res.Stream.ReadBuffer(data, 0, data.Length);
			return data;
		}

		public static void CopyValues(this IStreamingInfo destination, IStreamingInfo source)
		{
			destination.Offset = source.Offset;
			destination.Size = source.Size;
			destination.Path = source.Path;
		}
	}
}
