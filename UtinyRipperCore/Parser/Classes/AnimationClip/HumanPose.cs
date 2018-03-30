using System.Collections.Generic;

namespace UtinyRipper.Classes.AnimationClips
{
	public struct HumanPose : IAssetReadable
	{
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsVector3(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadTArray(Version version)
		{
			return version.IsGreaterEqual(5);
		}

		public void Read(AssetStream stream)
		{
			RootX.Read(stream);
			if(IsVector3(stream.Version))
			{
				LookAtPosition.Read3(stream);
			}
			else
			{
				LookAtPosition.Read(stream);
			}
			LookAtWeight.Read(stream);
			m_goalArray = stream.ReadArray<HumanGoal>();
			LeftHandPose.Read(stream);
			RightHandPose.Read(stream);
			m_doFArray = stream.ReadSingleArray();

			if(IsReadTArray(stream.Version))
			{
				if(IsVector3(stream.Version))
				{
					m_TDoFArray = stream.ReadVector3Array();
				}
				else
				{
					m_TDoFArray = stream.ReadArray<Vector4f>();
				}
			}
		}

		public IReadOnlyList<HumanGoal> GoalArray => m_goalArray;
		public IReadOnlyList<float> DoFArray => m_doFArray;
		public IReadOnlyList<Vector4f> TDoFArray => m_TDoFArray;

		public XForm RootX;
		public Vector4f LookAtPosition;
		public Vector4f LookAtWeight;
		public HandPose LeftHandPose;
		public HandPose RightHandPose;

		private HumanGoal[] m_goalArray;
		private float[] m_doFArray;
		private Vector4f[] m_TDoFArray;
	}
}
