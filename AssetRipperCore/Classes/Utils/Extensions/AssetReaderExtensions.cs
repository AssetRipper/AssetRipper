using AssetRipper.Classes.Misc.Serializable;
using AssetRipper.IO.Asset;
using AssetRipper.Math;

namespace AssetRipper.Classes.Utils.Extensions
{
	public static class AssetReaderExtensions
	{
		public static Vector4f[] ReadVector3Array(this AssetReader reader)
		{
			int count = reader.ReadInt32();
			Vector4f[] array = new Vector4f[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = reader.ReadAsset<Vector3f>();
			}
			return array;
		}
	}
}
