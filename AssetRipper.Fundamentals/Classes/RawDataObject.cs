using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Utils;

namespace AssetRipper.Core.Classes
{
	public abstract class RawDataObject : UnityObjectBase, IHasRawData
	{
		public sealed override string AssetClassName => ClassID.ToString();
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
