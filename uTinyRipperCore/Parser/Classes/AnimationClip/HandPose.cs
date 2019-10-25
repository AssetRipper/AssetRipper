using System.Collections.Generic;
using uTinyRipper.Classes.Misc;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct HandPose : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			GrabX.Read(reader);

			m_doFArray = reader.ReadSingleArray();
			Override = reader.ReadSingle();
			CloseOpen = reader.ReadSingle();
			InOut = reader.ReadSingle();
			Grab = reader.ReadSingle();
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
