using AssetRipper.Core.IO.Extensions;
using System.IO;

namespace AssetRipper.Core.Reading.Classes
{
	public class SerializedShaderDependency
	{
		public string from;
		public string to;

		public SerializedShaderDependency(BinaryReader reader)
		{
			from = reader.ReadAlignedString();
			to = reader.ReadAlignedString();
		}
	}
}
