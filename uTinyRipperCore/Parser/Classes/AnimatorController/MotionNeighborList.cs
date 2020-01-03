namespace uTinyRipper.Classes.AnimatorControllers
{
	public struct MotionNeighborList : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			NeighborArray = reader.ReadUInt32Array();
		}

		public uint[] NeighborArray { get; set; }
	}
}
