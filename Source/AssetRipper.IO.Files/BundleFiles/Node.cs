using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.Utils;

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
				PathFixed = FilenameUtils.FixFileIdentifier(value);
			}
		}
		public long Offset { get; set; }
		public long Size { get; set; }
	}
}
