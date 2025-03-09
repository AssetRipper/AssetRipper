using AssetRipper.IO.Endian;

namespace AssetRipper.IO.Files.BundleFiles
{
	public abstract record class Node : IEndianWritable
	{
		private string path = "";

		public override string ToString() => PathFixed;

		public abstract void Write(EndianWriter writer);

		public string PathFixed { get; private set; } = "";
		public string Path
		{
			get => path;
			set
			{
				path = value;
				PathFixed = SpecialFileNames.FixFileIdentifier(value);
			}
		}
		public long Offset { get; set; }
		public long Size { get; set; }
	}
}
