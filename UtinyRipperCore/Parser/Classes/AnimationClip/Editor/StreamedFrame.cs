using System.Collections.Generic;

namespace UtinyRipper.Classes.AnimationClips.Editor
{
	public struct StreamedFrame : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			Time = stream.ReadSingle();
			m_curves = stream.ReadArray<StreamedCurveKey>();
		}

		public float Time { get; set; }
		public IReadOnlyList<StreamedCurveKey> Curves => m_curves;

		private StreamedCurveKey[] m_curves;
	}
}
