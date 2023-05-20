using AssetRipper.Assets;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Utils;
using AssetRipper.SourceGenerated;

namespace AssetRipper.Import.Classes
{
	public abstract class RawDataObject : NullObject
	{
		public sealed override string ClassName => ((ClassIDType)ClassID).ToString();
		public byte[] RawData { get; }
		/// <summary>
		/// A Crc32 hash of <see cref="RawData"/>
		/// </summary>
		public uint RawDataHash => CrcUtils.CalculateDigest(RawData);

		public RawDataObject(AssetInfo assetInfo, byte[] data) : base(assetInfo)
		{
			RawData = data;
		}

		public sealed override void WriteEditor(AssetWriter writer) => writer.Write(RawData);

		public sealed override void WriteRelease(AssetWriter writer) => writer.Write(RawData);
	}
}
