using AssetRipper.IO.Files.Streams.Smart;
using AssetRipper.IO.Files.Utils;

namespace AssetRipper.IO.Files
{
	public abstract class File : IDisposable
	{
		public override string? ToString()
		{
			return string.IsNullOrEmpty(NameFixed) ? NameFixed : base.ToString();
		}

		public string FilePath { get; set; } = string.Empty;
		public string Name
		{
			get => name;
			set
			{
				name = value;
				NameFixed = FilenameUtils.FixFileIdentifier(value);
			}
		}
		public string NameFixed { get; private set; } = string.Empty;
		private string name = string.Empty;

		public abstract void Read(SmartStream stream);
		public abstract void Write(System.IO.Stream stream);
		public virtual void ReadContents() { }
		public virtual void ReadContentsRecursively() => ReadContents();

		~File() => Dispose(false);

		protected virtual void Dispose(bool disposing) { }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
