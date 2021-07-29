using AssetRipper.IO;

namespace AssetRipper.Reading.Classes
{
	public sealed class MeshFilter : Component
    {
        public PPtr<Mesh> m_Mesh;

        public MeshFilter(ObjectReader reader) : base(reader)
        {
            m_Mesh = new PPtr<Mesh>(reader);
        }
    }
}
