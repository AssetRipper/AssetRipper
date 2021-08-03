using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.Shader
{
	public struct ShaderBindChannel : IAssetReadable
	{
		public ShaderBindChannel(uint source, VertexComponent target)
		{
			Source = (byte)source;
			Target = target;
		}

		public void Read(AssetReader reader)
		{
			Source = reader.ReadByte();
			Target = (VertexComponent)reader.ReadByte();
		}

		/// <summary>
		/// ShaderChannel enum
		/// </summary>
		public byte Source { get; set; }
		public VertexComponent Target { get; set; }
	}
}
