using AssetRipper.Core;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Smart;
using AssetRipper.Core.Parser.Files.ResourceFiles;
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

		public override string AssetClassName => nameof(DummyAssetForLooseResourceFile);

		private readonly SmartStream smartStream;

		public DummyAssetForLooseResourceFile(ResourceFile associatedFile)
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
