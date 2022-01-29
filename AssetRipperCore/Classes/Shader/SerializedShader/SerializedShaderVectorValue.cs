using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedShaderVectorValue : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			X.Read(reader);
			Y.Read(reader);
			Z.Read(reader);
			W.Read(reader);
			Name = reader.ReadString();
		}

		public bool IsZero => X.IsZero && Y.IsZero && Z.IsZero && W.IsZero;

		public string Name { get; set; }

		public SerializedShaderFloatValue X = new();
		public SerializedShaderFloatValue Y = new();
		public SerializedShaderFloatValue Z = new();
		public SerializedShaderFloatValue W = new();
	}
}
