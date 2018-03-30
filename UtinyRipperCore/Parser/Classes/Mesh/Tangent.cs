namespace UtinyRipper.Classes.Meshes
{
	public struct Tangent : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			Normal.Read(stream);
			TangentValue.Read(stream);
			Handedness = stream.ReadSingle();
		}

		public float Handedness { get; private set; }

		public Vector3f Normal;
		public Vector3f TangentValue;
	}
}
