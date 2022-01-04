using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public class SerializedTextureProperty : IAssetReadable, ISerializedTextureProperty
	{
		public void Read(AssetReader reader)
		{
			DefaultName = reader.ReadString();
			TexDim = reader.ReadInt32();
		}

		public string DefaultName { get; set; }
		public int TexDim { get; set; }
	}
}
