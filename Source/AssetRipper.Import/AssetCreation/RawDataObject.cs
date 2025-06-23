using AssetRipper.Assets;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Checksum;
using AssetRipper.SourceGenerated;

namespace AssetRipper.Import.AssetCreation;

public abstract class RawDataObject : NullObject
{
	public sealed override string ClassName => ((ClassIDType)ClassID).ToString();
	public byte[] RawData { get; }
	/// <summary>
	/// A Crc32 hash of <see cref="RawData"/>
	/// </summary>
	public uint RawDataHash { get; }

	public RawDataObject(AssetInfo assetInfo, byte[] data) : base(assetInfo)
	{
		RawData = data;
		RawDataHash = Crc32Algorithm.HashData(data);
	}

	public sealed override void WriteEditor(AssetWriter writer) => writer.Write(RawData);

	public sealed override void WriteRelease(AssetWriter writer) => writer.Write(RawData);
}
