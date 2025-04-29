using AssetRipper.IO.Files.Streams.Smart;

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
			get;
			set
			{
				field = value;
				NameFixed = SpecialFileNames.FixFileIdentifier(value);
			}
		} = string.Empty;
		public string NameFixed { get; private set; } = string.Empty;

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
