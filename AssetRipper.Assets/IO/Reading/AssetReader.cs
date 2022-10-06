using AssetRipper.Assets.Collections;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.VersionUtilities;
using System.Text;


namespace AssetRipper.Assets.IO.Reading
{
	public sealed class AssetReader : EndianReader
	{
		public AssetReader(Stream stream, AssetCollection assetCollection) : base(stream, assetCollection.EndianType, false)
		{
			AssetCollection = assetCollection;
		}

		public override string ReadString()
		{
			throw new NotSupportedException();
		}

		public string ReadUnityString()
		{
			int length = ReadInt32();
			if (length == 0)
			{
				return string.Empty;
			}

			byte[] buffer = ReadStringBuffer(length);
			return Encoding.UTF8.GetString(buffer, 0, length);
		}

		public string ReadUnityString(bool align)
		{
			string result = ReadUnityString();
			if (align)
			{
				AlignStream();
			}
			return result;
		}

		public T ReadAsset<T>() where T : IAssetReadable, new()
		{
			T t = new();
			t.Read(this);
			return t;
		}

		public override string ToString()
		{
			return $"{nameof(AssetReader)} ({Platform} {Version})";
		}

		public UnityVersion Version => AssetCollection.Version;
		public BuildTarget Platform => AssetCollection.Platform;
		public TransferInstructionFlags Flags => AssetCollection.Flags;
		public AssetCollection AssetCollection { get; }
	}
}
