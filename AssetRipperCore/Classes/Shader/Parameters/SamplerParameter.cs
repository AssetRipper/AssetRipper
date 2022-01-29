using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.Shader.Parameters
{
	public sealed class SamplerParameter : IAssetReadable
	{
		public SamplerParameter() { }

		public SamplerParameter(uint sampler, int bindPoint)
		{
			Sampler = sampler;
			BindPoint = bindPoint;
		}

		public void Read(AssetReader reader)
		{
			Sampler = reader.ReadUInt32();
			BindPoint = reader.ReadInt32();
		}

		public uint Sampler { get; set; }
		public int BindPoint { get; set; }
	}
}
