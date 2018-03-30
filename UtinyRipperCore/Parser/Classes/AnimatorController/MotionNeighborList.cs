using System.Collections.Generic;

namespace UtinyRipper.Classes.AnimatorControllers
{
	public struct MotionNeighborList : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			m_neighborArray = stream.ReadUInt32Array();
		}

		public IReadOnlyList<uint> NeighborArray => m_neighborArray;

		private uint[] m_neighborArray;
	}
}
