using System.Collections.Generic;

namespace uTinyRipper.Classes.AnimationClips
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
		/// 5.2.0 and greater
		/// </summary>
		public static bool IsReadTArray(Version version)
		{
			return version.IsGreaterEqual(5, 2);
		}

		public void Read(AssetReader reader)
		{
			RootX.Read(reader);
			if(IsVector3(reader.Version))
			{
				LookAtPosition.Read3(reader);
			}
			else
			{
				LookAtPosition.Read(reader);
			}
			LookAtWeight.Read(reader);
			m_goalArray = reader.ReadAssetArray<HumanGoal>();
			LeftHandPose.Read(reader);
			RightHandPose.Read(reader);
			m_doFArray = reader.ReadSingleArray();

			if(IsReadTArray(reader.Version))
			{
				if(IsVector3(reader.Version))
				{
					m_TDoFArray = reader.ReadVector3Array();
				}
				else
				{
					m_TDoFArray = reader.ReadAssetArray<Vector4f>();
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
