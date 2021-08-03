using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math;

namespace AssetRipper.Core.Reading.Classes
{
	public class Transform : Component
    {
        public Quaternionf m_LocalRotation;
        public Vector3f m_LocalPosition;
        public Vector3f m_LocalScale;
        public PPtr<Transform>[] m_Children;
        public PPtr<Transform> m_Father;

        public Transform(ObjectReader reader) : base(reader)
        {
            m_LocalRotation = reader.ReadQuaternionf();
            m_LocalPosition = reader.ReadVector3f();
            m_LocalScale = reader.ReadVector3f();

            int m_ChildrenCount = reader.ReadInt32();
            m_Children = new PPtr<Transform>[m_ChildrenCount];
            for (int i = 0; i < m_ChildrenCount; i++)
            {
                m_Children[i] = new PPtr<Transform>(reader);
            }
            m_Father = new PPtr<Transform>(reader);
        }
    }
}
