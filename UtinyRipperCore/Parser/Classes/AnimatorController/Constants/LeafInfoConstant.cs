namespace UtinyRipper.Classes.AnimatorControllers
{
	public struct LeafInfoConstant : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			m_IDArray = stream.ReadUInt32Array();
			IndexOffset = stream.ReadUInt32();
		}

		public uint[] IDArray => m_IDArray;
		public uint IndexOffset { get; private set; }
		
		private uint[] m_IDArray;
	}
}
