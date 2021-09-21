using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public struct SerializedProperties : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			Props = reader.ReadAssetArray<SerializedProperty>();
		}

		public SerializedProperty[] Props { get; set; }
	}
}
