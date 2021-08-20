using AssetRipper.Core.IO.Extensions;
using System.IO;

namespace AssetRipper.Core.Reading.Classes
{
	public class SerializedShaderVectorValue
	{
		public SerializedShaderFloatValue x;
		public SerializedShaderFloatValue y;
		public SerializedShaderFloatValue z;
		public SerializedShaderFloatValue w;
		public string name;

		public SerializedShaderVectorValue(BinaryReader reader)
		{
			x = new SerializedShaderFloatValue(reader);
			y = new SerializedShaderFloatValue(reader);
			z = new SerializedShaderFloatValue(reader);
			w = new SerializedShaderFloatValue(reader);
			name = reader.ReadAlignedString();
		}
	}
}
