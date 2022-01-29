using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedShaderDependency : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			From = reader.ReadString();
			To = reader.ReadString();
		}

		public string From { get; set; }
		public string To { get; set; }
	}
}
