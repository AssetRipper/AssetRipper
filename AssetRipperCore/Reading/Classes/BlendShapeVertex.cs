using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math;

namespace AssetRipper.Core.Reading.Classes
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
