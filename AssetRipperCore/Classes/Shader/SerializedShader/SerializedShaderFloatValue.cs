using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedShaderFloatValue : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			Val = reader.ReadSingle();
			Name = reader.ReadString();
		}

		public bool IsZero => Val == 0.0f;
		public bool IsMax => Val == 255.0f;

		public float Val { get; set; }
		public string Name { get; set; }
	}
}
