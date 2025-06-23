namespace AssetRipper.Export.Modules.Shaders.ShaderBlob.Parameters;

public sealed record class SamplerParameter
{
	public SamplerParameter() { }

	public SamplerParameter(uint sampler, int bindPoint)
	{
		Sampler = sampler;
		BindPoint = bindPoint;
	}

	public uint Sampler { get; set; }
	public int BindPoint { get; set; }
}
