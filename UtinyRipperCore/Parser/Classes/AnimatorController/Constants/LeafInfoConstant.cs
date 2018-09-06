using System.Collections.Generic;

namespace UtinyRipper.Classes.AnimatorControllers
{
	public struct LeafInfoConstant : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			m_IDArray = reader.ReadUInt32Array();
			IndexOffset = (int)reader.ReadUInt32();
		}

		public IReadOnlyList<uint> IDArray => m_IDArray;
		public int IndexOffset { get; private set; }
		
		private uint[] m_IDArray;
	}
}
