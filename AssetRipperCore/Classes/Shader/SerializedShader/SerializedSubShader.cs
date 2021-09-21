using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public struct SerializedSubShader : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			Passes = reader.ReadAssetArray<SerializedPass>();
			Tags.Read(reader);
			LOD = reader.ReadInt32();
		}

		public SerializedPass[] Passes { get; set; }
		public int LOD { get; set; }

		public SerializedTagMap Tags;
	}
}
