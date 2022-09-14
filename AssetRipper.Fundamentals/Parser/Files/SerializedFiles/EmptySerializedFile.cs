using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;
using AssetRipper.Core.Structure;
using AssetRipper.IO.Endian;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Core.Parser.Files.SerializedFiles
{
	public sealed class EmptySerializedFile : ISerializedFile
	{
		public static EmptySerializedFile Shared { get; } = new();

		public IFileCollection Collection => throw new NotSupportedException();

		public EndianType EndianType => EndianType.LittleEndian;

		public string Name => throw new NotSupportedException();

		public LayoutInfo Layout => throw new NotSupportedException();

		public UnityVersion Version => throw new NotSupportedException();

		public BuildTarget Platform => BuildTarget.NoTarget;

		public TransferInstructionFlags Flags => TransferInstructionFlags.NoTransferInstructionFlags;

		public IReadOnlyList<FileIdentifier> Dependencies => throw new NotSupportedException();

		public PPtr<T> CreatePPtr<T>(T asset) where T : IUnityObjectBase => throw new NotSupportedException();

		public IEnumerable<IUnityObjectBase> FetchAssets() => Enumerable.Empty<IUnityObjectBase>();

		public IUnityObjectBase? TryGetAsset(long pathID) => null;

		public IUnityObjectBase? TryGetAsset(int fileIndex, long pathID) => null;

		public IUnityObjectBase GetAsset(long pathID) => throw new NotSupportedException();

		public IUnityObjectBase GetAsset(int fileIndex, long pathID) => throw new NotSupportedException();

		public ObjectInfo GetAssetEntry(long pathID) => throw new NotSupportedException();
	}
}
