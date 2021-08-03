using AssetRipper.Core.IO.Asset;
using System.IO;

namespace AssetRipper.Core.Classes.Shader.Parameters
{
	public struct SamplerParameter : IAssetReadable
	{
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

		public SamplerParameter(BinaryReader reader)
		{
			Sampler = reader.ReadUInt32();
			BindPoint = reader.ReadInt32();
		}

		public uint Sampler { get; set; }
		public int BindPoint { get; set; }
	}
}
