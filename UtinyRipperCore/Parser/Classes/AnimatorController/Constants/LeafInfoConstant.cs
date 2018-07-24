using System.Collections.Generic;

namespace UtinyRipper.Classes.AnimatorControllers
{
	public struct LeafInfoConstant : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			m_IDArray = stream.ReadUInt32Array();
			IndexOffset = (int)stream.ReadUInt32();
		}

		public IReadOnlyList<uint> IDArray => m_IDArray;
		public int IndexOffset { get; private set; }
		
		private uint[] m_IDArray;
	}
}
