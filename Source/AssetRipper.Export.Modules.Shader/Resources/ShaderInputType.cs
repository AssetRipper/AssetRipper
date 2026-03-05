namespace AssetRipper.Export.Modules.Shaders.Resources;

internal enum ShaderInputType
{
	CBuffer = 0,
	TBuffer = 1,
	Texture = 2,
	Sampler = 3,
	UavRwTyped = 4,
	Structured = 5,
	UavRwStructured = 6,
	ByteAddress = 7,
	UavRwByteAddress = 8,
	UavAppendStructured = 9,
	UavConsumeStructured = 10,
	UavRwStructuredWithCounter = 11
}