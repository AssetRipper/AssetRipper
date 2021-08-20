using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math;

namespace AssetRipper.Core.Reading.Classes
{
	public class HumanPose
    {
        public XForm m_RootX;
		public Vector3f m_LookAtPosition;
        public Vector4f m_LookAtWeight;
        public HumanGoal[] m_GoalArray;
        public HandPose m_LeftHandPose;
        public HandPose m_RightHandPose;
        public float[] m_DoFArray;
        public Vector3f[] m_TDoFArray;

        public HumanPose(ObjectReader reader)
        {
            var version = reader.version;
            m_RootX = new XForm(reader);
            m_LookAtPosition = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3f() : (Vector3f)reader.ReadVector4f();//5.4 and up
            m_LookAtWeight = reader.ReadVector4f();

            int numGoals = reader.ReadInt32();
            m_GoalArray = new HumanGoal[numGoals];
            for (int i = 0; i < numGoals; i++)
            {
                m_GoalArray[i] = new HumanGoal(reader);
            }

            m_LeftHandPose = new HandPose(reader);
            m_RightHandPose = new HandPose(reader);

            m_DoFArray = reader.ReadSingleArray();

            if (version[0] > 5 || (version[0] == 5 && version[1] >= 2))//5.2 and up
            {
                int numTDof = reader.ReadInt32();
                m_TDoFArray = new Vector3f[numTDof];
                for (int i = 0; i < numTDof; i++)
                {
                    m_TDoFArray[i] = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3f() : (Vector3f)reader.ReadVector4f();//5.4 and up
                }
            }
        }
    }
}
