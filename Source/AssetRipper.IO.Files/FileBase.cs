using AssetRipper.IO.Files.Streams.Smart;
using AssetRipper.IO.Files.Utils;

namespace AssetRipper.IO.Files
{
	/// <summary>
	/// The base class for files.
	/// </summary>
	public abstract class FileBase : IDisposable
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
		public abstract void Write(Stream stream);
		public virtual void ReadContents() { }
		public virtual void ReadContentsRecursively() => ReadContents();
		public virtual byte[] ToByteArray()
		{
			MemoryStream memoryStream = new();
			Write(memoryStream);
			return memoryStream.ToArray();
		}

		~FileBase() => Dispose(false);

		protected virtual void Dispose(bool disposing) { }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
