using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;

namespace AssetRipper.Core.Classes.Shader.Parameters
{
	public struct BufferBinding : IAssetReadable
	{
		public BufferBinding(string name, int index)
		{
			Name = name;
			NameIndex = -1;
			Index = index;
			ArraySize = 0;
		}

		public void Read(AssetReader reader)
		{
			NameIndex = reader.ReadInt32();
			Index = reader.ReadInt32();
			if(HasArraySize(reader.Version))
			{
				ArraySize = reader.ReadInt32();
			}
		}

		/// <summary>
		/// 2020 and greater
		/// </summary>
		public static bool HasArraySize(UnityVersion version) => version.IsGreaterEqual(2020);

		public string Name { get; set; }
		public int NameIndex { get; set; }
		public int Index { get; set; }
		public int ArraySize { get; set; }
	}
}
