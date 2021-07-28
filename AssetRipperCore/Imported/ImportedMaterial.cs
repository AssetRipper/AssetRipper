using AssetRipper.Math;
using System.Collections.Generic;

namespace AssetRipper.Imported
{
	public class ImportedMaterial
	{
		public string Name { get; set; }
		public Color Diffuse { get; set; }
		public Color Ambient { get; set; }
		public Color Specular { get; set; }
		public Color Emissive { get; set; }
		public Color Reflection { get; set; }
		public float Shininess { get; set; }
		public float Transparency { get; set; }
		public List<ImportedMaterialTexture> Textures { get; set; }
	}
}
