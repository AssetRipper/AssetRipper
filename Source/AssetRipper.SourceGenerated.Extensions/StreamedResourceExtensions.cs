using AssetRipper.Assets.Collections;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.SourceGenerated.Subclasses.StreamedResource;

namespace AssetRipper.SourceGenerated.Extensions;

public static class StreamedResourceExtensions
{
	internal static bool CheckIntegrity(Utf8String? path, ulong offset, ulong size, AssetCollection collection)
	{
		if (Utf8String.IsNullOrEmpty(path))
		{
			return true;
		}

		if (offset > long.MaxValue || size > long.MaxValue || offset + size > long.MaxValue)
		{
			return false;
		}

		if (size == 0)
		{
			// Data might be read by its type for this verison, so we can't even export raw data.
			return false;
		}

		ResourceFile? file = collection.Bundle.ResolveResource(path.String);
		if (file == null)
		{
			return false;
		}

		return file.Stream.Length >= unchecked((long)(offset + size));
	}

	internal static byte[]? GetContent(Utf8String? path, ulong offset, ulong size, AssetCollection collection)
	{
		if (Utf8String.IsNullOrEmpty(path))
		{
			return null;
		}

		if (offset > long.MaxValue || size > long.MaxValue || offset + size > long.MaxValue)
		{
			return null;
		}

		if (size == 0)
		{
			// Data might be read by its type for this verison, so we can't even export raw data.
			return null;
		}

		ResourceFile? file = collection.Bundle.ResolveResource(path.String);
		if (file == null || file.Stream.Length < unchecked((long)(offset + size)))
		{
			return null;
		}

		byte[] data = new byte[size];
		file.Stream.Position = (long)offset;
		file.Stream.ReadExactly(data);
		return data;
	}

	public static bool CheckIntegrity(this IStreamedResource streamedResource, AssetCollection collection)
	{
		return CheckIntegrity(streamedResource.Source, streamedResource.Offset, streamedResource.Size, collection);
	}

	public static byte[]? GetContent(this IStreamedResource streamedResource, AssetCollection file)
	{
		return GetContent(streamedResource.Source, streamedResource.Offset, streamedResource.Size, file);
	}

	public static bool TryGetContent(this IStreamedResource streamedResource, AssetCollection file, [NotNullWhen(true)] out byte[]? data)
	{
		data = streamedResource.GetContent(file);
		return !data.IsNullOrEmpty();
	}

	public static bool IsSet(this IStreamedResource streamedResource) => !Utf8String.IsNullOrEmpty(streamedResource.Source);
}
