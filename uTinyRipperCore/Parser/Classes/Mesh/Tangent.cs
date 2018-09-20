namespace uTinyRipper.Classes.Meshes
{
	public struct Tangent : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			Normal.Read(reader);
			TangentValue.Read(reader);
			Handedness = reader.ReadSingle();
		}

		public float Handedness { get; private set; }

		public Vector3f Normal;
		public Vector3f TangentValue;
	}
}
