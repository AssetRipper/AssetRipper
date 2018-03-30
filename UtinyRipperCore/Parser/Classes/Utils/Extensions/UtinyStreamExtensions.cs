namespace UtinyRipper.Classes
{
	public static class UtinyStreamExtensions
	{
		public static Vector4f[] ReadVector3Array(this AssetStream stream)
		{
			int count = stream.ReadInt32();
			Vector4f[] array = new Vector4f[count];
			for(int i = 0; i < count; i++)
			{
				Vector4f vector = new Vector4f();
				vector.Read3(stream);
				array[i] = vector;
			}
			return array;
		}
	}
}
