using AssetRipper.IO.Endian;
using AssetRipper.Primitives;

namespace AssetRipper.IO.Files.SerializedFiles.FileIdentifiers;

internal static class EndianExtensions
{
	public static UnityGUID ReadUnityGUID(this EndianReader reader)
	{
		return new UnityGUID(reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32());
	}

	public static void Write(this EndianWriter writer, UnityGUID guid)
	{
		writer.Write(guid.Data0);
		writer.Write(guid.Data1);
		writer.Write(guid.Data2);
		writer.Write(guid.Data3);
	}
}
