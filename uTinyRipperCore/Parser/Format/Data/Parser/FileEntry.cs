namespace uTinyRipper
{
	public abstract class FileEntry
	{
		public override string ToString()
		{
			return Name;
		}

		public string Name { get; protected set; }
		public string NameOrigin { get; protected set; }
		public long Offset { get; protected set; }
		public long Size { get; protected set; }
	}
}
