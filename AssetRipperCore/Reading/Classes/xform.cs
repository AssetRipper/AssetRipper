using AssetRipper.IO.Extensions;
using AssetRipper.Math;

namespace AssetRipper.Reading.Classes
{
	public class xform
    {
        public Vector3f t;
        public Quaternionf q;
        public Vector3f s;

        public xform(ObjectReader reader)
        {
            var version = reader.version;
            t = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3f() : (Vector3f)reader.ReadVector4f();//5.4 and up
            q = reader.ReadQuaternionf();
            s = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3f() : (Vector3f)reader.ReadVector4f();//5.4 and up
        }
    }
}
