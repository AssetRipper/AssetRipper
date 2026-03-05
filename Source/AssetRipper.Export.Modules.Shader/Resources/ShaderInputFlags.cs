namespace AssetRipper.Export.Modules.Shaders.Resources;

[Flags]
internal enum ShaderInputFlags
{
	None = 0x0,
	UserPacked = 0x1,
	ComparisonSampler = 0x2,
	TextureComponent0 = 0x4,
	TextureComponent1 = 0x8,
	TextureComponents = TextureComponent0 | TextureComponent1,
	Unused = 0x10,
}