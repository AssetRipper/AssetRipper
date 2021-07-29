using AssetRipper.IO;
using AssetRipper.IO.Extensions;
using AssetRipper.Math;

namespace AssetRipper.Reading.Classes
{
	public class BlendShapeVertex
    {
        public Vector3f vertex;
        public Vector3f normal;
        public Vector3f tangent;
        public uint index;

        public BlendShapeVertex(ObjectReader reader)
        {
            vertex = reader.ReadVector3f();
            normal = reader.ReadVector3f();
            tangent = reader.ReadVector3f();
            index = reader.ReadUInt32();
        }
    }
}
