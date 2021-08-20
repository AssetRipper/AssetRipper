using AssetRipper.Core.IO.Extensions;
using System.IO;

namespace AssetRipper.Core.Reading.Classes
{
	public class SerializedShaderFloatValue
	{
		public float val;
		public string name;

		public SerializedShaderFloatValue(BinaryReader reader)
		{
			val = reader.ReadSingle();
			name = reader.ReadAlignedString();
		}
	}
}
