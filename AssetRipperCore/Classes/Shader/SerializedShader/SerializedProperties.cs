using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedProperties : IAssetReadable, ISerializedProperties
	{
		public void Read(AssetReader reader)
		{
			m_Props = reader.ReadAssetArray<SerializedProperty>();
		}

		private SerializedProperty[] m_Props;

		public ISerializedProperty[] Props => m_Props;
	}
}
