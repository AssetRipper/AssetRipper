using System.Collections.Generic;

namespace AssetRipper.Core.Imported
{
	public class ImportedMorphKeyframe
	{
		public bool hasNormals { get; set; }
		public bool hasTangents { get; set; }
		public float Weight { get; set; }
		public List<ImportedMorphVertex> VertexList { get; set; }
	}
}
