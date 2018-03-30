using System.Collections.Generic;

namespace UtinyRipper.Classes.AnimationClips
{
	public struct ConstantClip : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			m_constants = stream.ReadSingleArray();
		}
		
		public bool IsValid => Constants.Count > 0;

		public IReadOnlyList<float> Constants => m_constants;

		private float[] m_constants;
	}
}
