namespace DXShaderExporter
{
	internal enum ShaderInputFlags
	{
		None,
		UserPacked = 0x1,
		ComparisonSampler = 0x2,
		TextureComponent0 = 0x4,
		TextureComponent1 = 0x8,
		TextureComponents = 0xc,
		Unused = 0x10
	}
}