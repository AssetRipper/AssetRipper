namespace AssetRipper.IO.Files.Extensions;

public static class StreamExtensions
{
	public static void Align(this Stream _this) => _this.Align(4);
	public static void Align(this Stream _this, int alignment)
	{
		long pos = _this.Position;
		long mod = pos % alignment;
		if (mod != 0)
		{
			_this.Position += alignment - mod;
		}
	}
}
