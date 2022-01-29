using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.Shader.Parameters
{
	public sealed class UAVParameter : IAssetReadable
	{
		public UAVParameter() { }

		public UAVParameter(string name, int index, int originalIndex)
		{
			Name = name;
			NameIndex = -1;
			Index = index;
			OriginalIndex = originalIndex;
		}

		public void Read(AssetReader reader)
		{
			NameIndex = reader.ReadInt32();
			Index = reader.ReadInt32();
			OriginalIndex = reader.ReadInt32();
		}

		public string Name { get; set; }
		public int NameIndex { get; set; }
		public int Index { get; set; }
		public int OriginalIndex { get; set; }
	}
}
