using System.Collections.Generic;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct ConstantClip : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			m_constants = reader.ReadSingleArray();
		}
		
		public bool IsSet => Constants.Count > 0;

		public IReadOnlyList<float> Constants => m_constants;

		private float[] m_constants;
	}
}
