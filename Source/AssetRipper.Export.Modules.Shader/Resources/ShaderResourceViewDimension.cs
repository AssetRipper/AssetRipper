namespace AssetRipper.Export.Modules.Shaders.Resources;

internal enum ShaderResourceViewDimension
{
	Unknown = 0,
	Buffer = 1,
	Texture1D = 2,
	Texture1DArray = 3,
	Texture2D = 4,
	Texture2DArray = 5,
	Texture2DMultiSampled = 6,
	Texture2DMultiSampledArray = 7,
	Texture3D = 8,
	TextureCube = 9,
	TextureCubeArray = 10,
	ExtendedBuffer = 11,
}