namespace UtinyRipper.Classes.Sprites
{
	public struct SpriteBone : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			Name = stream.ReadStringAligned();
			Position.Read(stream);
			Rotation.Read(stream);
			Length = stream.ReadSingle();
			ParentID = stream.ReadInt32();
		}

		public string Name { get; private set; }
		public float Length { get; private set; }
		public int ParentID { get; private set; }

		public Vector3f Position;
		public Quaternionf Rotation;
	}
}
