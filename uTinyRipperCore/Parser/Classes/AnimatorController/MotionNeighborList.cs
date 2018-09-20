using System.Collections.Generic;

namespace uTinyRipper.Classes.AnimatorControllers
{
	public struct MotionNeighborList : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			m_neighborArray = reader.ReadUInt32Array();
		}

		public IReadOnlyList<uint> NeighborArray => m_neighborArray;

		private uint[] m_neighborArray;
	}
}
