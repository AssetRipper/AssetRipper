using AssetRipper.IO.Endian;

namespace AssetRipper.IO.Files.SerializedFiles.FileIdentifiers;

internal static class EndianExtensions
{
	public static UnityGuid ReadUnityGuid(this EndianReader reader)
	{
		return new UnityGuid(reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32());
	}

	public static void Write(this EndianWriter writer, UnityGuid guid)
	{
		writer.Write(guid.Data0);
		writer.Write(guid.Data1);
		writer.Write(guid.Data2);
		writer.Write(guid.Data3);
	}
}
