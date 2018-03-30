using System.Collections.Generic;

namespace UtinyRipper.Classes.AnimationClips
{
	public struct HandPose : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			GrabX.Read(stream);

			m_doFArray = stream.ReadSingleArray();
			Override = stream.ReadSingle();
			CloseOpen = stream.ReadSingle();
			InOut = stream.ReadSingle();
			Grab = stream.ReadSingle();
		}

		public IReadOnlyList<float> DoFArray => m_doFArray;
		public float Override { get; private set; }
		public float CloseOpen { get; private set; }
		public float InOut { get; private set; }
		public float Grab { get; private set; }

		public XForm GrabX;

		private float[] m_doFArray;
	}
}
