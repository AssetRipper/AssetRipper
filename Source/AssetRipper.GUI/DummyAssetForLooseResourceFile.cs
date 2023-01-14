using AssetRipper.Assets;
using AssetRipper.Assets.Interfaces;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.Streams.Smart;
using System.IO;
using System.Threading.Tasks;

namespace AssetRipper.GUI
{
	public sealed class DummyAssetForLooseResourceFile : UnityObjectBase, IDisposable, IHasNameString
	{
		private bool disposedValue;

		public ResourceFile AssociatedFile { get; }

		public string NameString
		{
			get => AssociatedFile.Name;
			set => throw new NotSupportedException();
		}

		public override string ClassName => nameof(DummyAssetForLooseResourceFile);

		private readonly SmartStream smartStream;

		public DummyAssetForLooseResourceFile(ResourceFile associatedFile) : base(Assets.Metadata.AssetInfo.MakeDummyAssetInfo(-1))
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
	}
}
