using AssetRipper.Core.Math;

namespace AssetRipper.Core.Imported
{
	public class ImportedMaterialTexture
	{
		public string Name { get; set; }
		public int Dest { get; set; }
		public Vector2f Offset { get; set; }
		public Vector2f Scale { get; set; }
	}
}
