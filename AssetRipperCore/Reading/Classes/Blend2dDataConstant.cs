using AssetRipper.Classes.AnimatorController;
using AssetRipper.IO;
using AssetRipper.IO.Extensions;
using AssetRipper.Math;

namespace AssetRipper.Reading.Classes
{
	public class Blend2dDataConstant
    {
        public Vector2f[] m_ChildPositionArray;
        public float[] m_ChildMagnitudeArray;
        public Vector2f[] m_ChildPairVectorArray;
        public float[] m_ChildPairAvgMagInvArray;
        public MotionNeighborList[] m_ChildNeighborListArray;

        public Blend2dDataConstant(ObjectReader reader)
        {
            m_ChildPositionArray = reader.ReadVector2Array();
            m_ChildMagnitudeArray = reader.ReadSingleArray();
            m_ChildPairVectorArray = reader.ReadVector2Array();
            m_ChildPairAvgMagInvArray = reader.ReadSingleArray();

            int numNeighbours = reader.ReadInt32();
            m_ChildNeighborListArray = new MotionNeighborList[numNeighbours];
            for (int i = 0; i < numNeighbours; i++)
            {
                m_ChildNeighborListArray[i] = new MotionNeighborList(reader);
            }
        }
    }
}
