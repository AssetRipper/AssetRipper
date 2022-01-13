using AssetRipper.Core;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Smart;
using AssetRipper.Core.Parser.Files.ResourceFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AssetRipper.GUI
{
	public class DummyAssetForLooseResourceFile : UnityObjectBase, IDisposable, IHasName
	{
		private bool disposedValue;

		public ResourceFile AssociatedFile { get; }

		public string Name
		{
			get => AssociatedFile.Name;
			set => throw new NotSupportedException();
		}

		private readonly SmartStream smartStream;

		public DummyAssetForLooseResourceFile(ResourceFile associatedFile)
		{
			AssociatedFile = associatedFile;
			smartStream = SmartStream.CreateTemp();
			AssociatedFile.Stream.CopyTo(smartStream);
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public void SaveToFile(string path)
		{
			using FileStream fileStream = System.IO.File.Create(path);
			smartStream.Position = 0;
			smartStream.CopyTo(fileStream);
		}

		public async Task SaveToFileAsync(string path)
		{
			FileStream fileStream = System.IO.File.Create(path);
			smartStream.Position = 0;
			await smartStream.CopyToAsync(fileStream);
		}

		protected virtual void Dispose(bool disposing)
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