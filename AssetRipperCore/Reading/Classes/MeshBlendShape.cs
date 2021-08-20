using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Extensions;

namespace AssetRipper.Core.Reading.Classes
{
	public class MeshBlendShape
	{
		public uint firstVertex;
		public uint vertexCount;
		public bool hasNormals;
		public bool hasTangents;

		public MeshBlendShape(ObjectReader reader)
		{
			var version = reader.version;

			if (version[0] == 4 && version[1] < 3) //4.3 down
			{
				var name = reader.ReadAlignedString();
			}
			firstVertex = reader.ReadUInt32();
			vertexCount = reader.ReadUInt32();
			if (version[0] == 4 && version[1] < 3) //4.3 down
			{
				var aabbMinDelta = reader.ReadVector3f();
				var aabbMaxDelta = reader.ReadVector3f();
			}
			hasNormals = reader.ReadBoolean();
			hasTangents = reader.ReadBoolean();
			if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
			{
				reader.AlignStream();
			}
		}
	}
}
