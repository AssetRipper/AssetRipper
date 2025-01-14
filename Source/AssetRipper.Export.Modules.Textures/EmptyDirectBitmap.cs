namespace AssetRipper.Export.Modules.Textures;

internal sealed class EmptyDirectBitmap : DirectBitmap
{
	public static EmptyDirectBitmap Instance { get; } = new();

	private EmptyDirectBitmap() : base(0, 0, 1, [])
	{
	}

	public override int PixelSize => 0;

	public override EmptyDirectBitmap GetLayer(int layer) => this;

	public override void FlipX()
	{
	}

	public override void FlipY()
	{
	}

	public override void Transpose()
	{
	}

	public override EmptyDirectBitmap DeepClone() => this;

	public override void SaveAsBmp(Stream stream)
	{
	}

	public override void SaveAsExr(Stream stream)
	{
	}

	public override void SaveAsHdr(Stream stream)
	{
	}

	public override void SaveAsPng(Stream stream)
	{
	}

	public override void SaveAsJpeg(Stream stream)
	{
	}

	public override void SaveAsTga(Stream stream)
	{
	}
}
