using AssetRipper.Assets;
using AssetRipper.Assets.Interfaces;
using AssetRipper.Assets.IO.Reading;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Utils;
using AssetRipper.SourceGenerated;

namespace AssetRipper.Import.Classes
{
	public abstract class RawDataObject : NullObject, IHasRawData
	{
		public sealed override string ClassName => ((ClassIDType)ClassID).ToString();
		public byte[] RawData { get; private set; } = Array.Empty<byte>();
		/// <summary>
		/// A Crc32 hash of <see cref="RawData"/>
		/// </summary>
		public uint RawDataHash => CrcUtils.CalculateDigest(RawData);

		public RawDataObject(AssetInfo assetInfo) : base(assetInfo) { }

		public void Read(AssetReader reader, int byteSize)
		{
			if (byteSize > 0)
			{
				RawData = reader.ReadBytes(byteSize);
			}
			else
			{
				RawData = Array.Empty<byte>();
			}
		}

		public sealed override void WriteEditor(AssetWriter writer) => writer.Write(RawData);

		public sealed override void WriteRelease(AssetWriter writer) => writer.Write(RawData);
	}
}
