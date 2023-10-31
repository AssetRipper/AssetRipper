using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.GUI
{
	public sealed class DummyAssetForLooseResourceFile : UnityObjectBase, IDisposable, INamed
	{
		private bool disposedValue;

		public ResourceFile AssociatedFile { get; }

		public Utf8String Name
		{
			get => AssociatedFile.Name;
			set => throw new NotSupportedException();
		}

		public override string ClassName => nameof(DummyAssetForLooseResourceFile);

		private readonly SmartStream smartStream;

		public DummyAssetForLooseResourceFile(ResourceFile associatedFile) : base(MakeDummyAssetInfo())
		{
			AssociatedFile = associatedFile;
			smartStream = AssociatedFile.Stream.CreateReference();
		}

		public void SaveToFile(string path)
		{
			using FileStream fileStream = File.Create(path);
			smartStream.Position = 0;
			smartStream.CopyTo(fileStream);
		}

		public async Task SaveToFileAsync(string path)
		{
			FileStream fileStream = File.Create(path);
			smartStream.Position = 0;
			await smartStream.CopyToAsync(fileStream);
			await fileStream.FlushAsync();
		}

		private void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					smartStream?.Dispose();
				}

				AssociatedFile?.Dispose();
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		private static AssetInfo MakeDummyAssetInfo()
		{
			return new AssetInfo(dummyBundle.Collection, 0, -1);
		}

		private static readonly DummyBundle dummyBundle = new();

		private sealed class DummyAssetCollection : AssetCollection
		{
			public DummyAssetCollection(Bundle bundle) : base(bundle)
			{
			}
		}

		private sealed class DummyBundle : Bundle
		{
			public DummyAssetCollection Collection { get; }
			public override string Name => nameof(DummyBundle);
			public DummyBundle()
			{
				Collection = new DummyAssetCollection(this);
			}
		}
	}
}
