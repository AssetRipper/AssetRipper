using AssetRipper.Core.Math;
using System.Collections.Generic;

namespace AssetRipper.Core.Imported
{
	public class ImportedMaterial
	{
		public string Name { get; set; }
		public ColorRGBAf Diffuse { get; set; }
		public ColorRGBAf Ambient { get; set; }
		public ColorRGBAf Specular { get; set; }
		public ColorRGBAf Emissive { get; set; }
		public ColorRGBAf Reflection { get; set; }
		public float Shininess { get; set; }
		public float Transparency { get; set; }
		public List<ImportedMaterialTexture> Textures { get; set; }
	}
}
