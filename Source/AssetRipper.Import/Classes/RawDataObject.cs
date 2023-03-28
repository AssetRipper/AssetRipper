using AssetRipper.Assets;
using AssetRipper.Assets.Interfaces;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Utils;
using AssetRipper.IO.Endian;
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

		public void Read(ref EndianSpanReader reader, int byteSize)
		{
			RawData = reader.ReadBytesExact(byteSize);
		}

		public sealed override void WriteEditor(AssetWriter writer) => writer.Write(RawData);

		public sealed override void WriteRelease(AssetWriter writer) => writer.Write(RawData);
	}
}
