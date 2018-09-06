using System.Collections.Generic;

namespace UtinyRipper.Classes.AnimationClips.Editor
{
	public struct StreamedFrame : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			Time = reader.ReadSingle();
			m_curves = reader.ReadArray<StreamedCurveKey>();
		}

		public float Time { get; set; }
		public IReadOnlyList<StreamedCurveKey> Curves => m_curves;

		private StreamedCurveKey[] m_curves;
	}
}
