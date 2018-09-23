namespace uTinyRipper.Classes.Sprites
{
	public struct SpriteBone : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			Name = reader.ReadString();
			Position.Read(reader);
			Rotation.Read(reader);
			Length = reader.ReadSingle();
			ParentID = reader.ReadInt32();
		}

		public string Name { get; private set; }
		public float Length { get; private set; }
		public int ParentID { get; private set; }

		public Vector3f Position;
		public Quaternionf Rotation;
	}
}
