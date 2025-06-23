namespace AssetRipper.Export.Modules.Shaders.Resources;

public enum DXProgramType
{
	PixelShader = 0xFFFF,
	VertexShader = 0xFFFE,
	GeometryShader = 0x4753,
	HullShader = 0x4853,
	DomainShader = 0x4453,
	ComputeShader = 0x4353,
}