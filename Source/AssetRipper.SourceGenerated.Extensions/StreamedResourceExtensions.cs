using AssetRipper.Assets.Collections;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.SourceGenerated.Subclasses.StreamedResource;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class StreamedResourceExtensions
	{
		public static bool CheckIntegrity(this IStreamedResource streamedResource, AssetCollection file)
		{
			if (!streamedResource.IsSet())
			{
				return true;
			}
			if (streamedResource.Size == 0)
			{
				// I think they read data by its type for this verison, so I can't even export raw data :/
				return false;
			}

			return file.Bundle.ResolveResource(streamedResource.Source.String) != null;
		}

		public static byte[]? GetContent(this IStreamedResource streamedResource, AssetCollection file)
		{
			ResourceFile? res = file.Bundle.ResolveResource(streamedResource.Source.String);
			if (res == null)
			{
				return null;
			}
			if (streamedResource.Size == 0)
			{
				return null;
			}

			byte[] data = new byte[streamedResource.Size];
			res.Stream.Position = (long)streamedResource.Offset;
			res.Stream.ReadExactly(data);
			return data;
		}

		public static bool TryGetContent(this IStreamedResource streamedResource, AssetCollection file, [NotNullWhen(true)] out byte[]? data)
		{
			data = streamedResource.GetContent(file);
			return !data.IsNullOrEmpty();
		}

		public static bool IsSet(this IStreamedResource streamedResource) => !string.IsNullOrEmpty(streamedResource.Source?.String);
	}
}
