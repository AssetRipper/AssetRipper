using AssetRipper.Assets;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.ResourceFiles;

#nullable disable

namespace AssetRipper.AssemblyDumper.InjectedTypes;

internal static class AudioClipHelper
{
	internal static byte[] ReadOldByteArray(this UnityObjectBase audioClip, ref EndianSpanReader reader, int m_Stream)
	{
		return m_Stream == 2 //AudioClipLoadType.Streaming
			? ReadStreamedByteArray(audioClip, ref reader)
			: ReadAlignedByteArray(ref reader);
	}

	private static byte[] ReadAlignedByteArray(ref EndianSpanReader reader)
	{
		int count = reader.ReadInt32();
		byte[] result = reader.ReadBytesExact(count).ToArray();
		reader.Align();
		return result;
	}

	private static byte[] ReadStreamedByteArray(UnityObjectBase audioClip, ref EndianSpanReader reader)
	{
		uint size = reader.ReadUInt32();
		uint offset = reader.ReadUInt32();
		string resourceFileName = audioClip.Collection.Name + ".resS";
		if (TryFindResourceFile(audioClip, resourceFileName, out ResourceFile resourceFile))
		{
			byte[] result = new byte[size];
			resourceFile.Stream.Position = offset;
			resourceFile.Stream.ReadExactly(result);
			return result;
		}
		else
		{
			return Array.Empty<byte>();
		}
	}

	private static bool TryFindResourceFile(UnityObjectBase audioClip, string resourceFileName, out ResourceFile resourceFile)
	{
		resourceFile = audioClip.Collection.Bundle.ResolveResource(resourceFileName);
		return resourceFile is not null;
	}
}

#nullable enable