using AssetRipper.Core.Classes.Shader.Enums;

namespace ShaderTextRestorer.ShaderBlob
{
	public sealed class ShaderBindChannel
	{
		public ShaderBindChannel() { }

		public ShaderBindChannel(uint source, VertexComponent target)
		{
			Source = source;
			Target = target;
		}

		/// <summary>
		/// ShaderChannel enum
		/// </summary>
		public uint Source { get; set; }
		public VertexComponent Target { get; set; }
	}
}
