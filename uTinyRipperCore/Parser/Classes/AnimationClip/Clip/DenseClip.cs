using System.Collections.Generic;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct DenseClip : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			FrameCount = reader.ReadInt32();
			CurveCount = (int)reader.ReadUInt32();
			SampleRate = reader.ReadSingle();
			BeginTime = reader.ReadSingle();

			m_sampleArray = reader.ReadSingleArray();
		}

		public bool IsValid => SampleArray.Count > 0;

		public int FrameCount { get; private set; }
		public int CurveCount { get; private set; }
		public float SampleRate { get; private set; }
		public float BeginTime { get; private set; }
		public IReadOnlyList<float> SampleArray => m_sampleArray;

		private float[] m_sampleArray;
	}
}
