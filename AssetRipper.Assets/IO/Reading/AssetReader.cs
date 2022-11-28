using AssetRipper.Assets.Collections;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.VersionUtilities;
using System.IO;
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
			int length = ReadInt32();

			if (length == 0)
			{
				return string.Empty;
			}

			string ret = base.ReadString(length);
			AlignStream();
			//Strings have supposedly been aligned since 2.1.0,
			//which is earlier than the beginning of AssetRipper version support.
			return ret;
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
