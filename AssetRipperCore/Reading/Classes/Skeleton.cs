using AssetRipper.IO;
using AssetRipper.IO.Extensions;

namespace AssetRipper.Reading.Classes
{
	public class Skeleton
    {
        public Node[] m_Node;
        public uint[] m_ID;
        public Axes[] m_AxesArray;


        public Skeleton(ObjectReader reader)
        {
            int numNodes = reader.ReadInt32();
            m_Node = new Node[numNodes];
            for (int i = 0; i < numNodes; i++)
            {
                m_Node[i] = new Node(reader);
            }

            m_ID = reader.ReadUInt32Array();

            int numAxes = reader.ReadInt32();
            m_AxesArray = new Axes[numAxes];
            for (int i = 0; i < numAxes; i++)
            {
                m_AxesArray[i] = new Axes(reader);
            }
        }
    }
}
