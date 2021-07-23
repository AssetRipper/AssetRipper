using AssetRipper.Parser.Classes.Shader.Enums;
using AssetRipper.IO.Asset;

namespace AssetRipper.Parser.Classes.Shader
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
