using System.Collections.Generic;

namespace AssetRipper.Imported
{
	public class ImportedMesh
	{
		public string Path { get; set; }
		public List<ImportedVertex> VertexList { get; set; }
		public List<ImportedSubmesh> SubmeshList { get; set; }
		public List<ImportedBone> BoneList { get; set; }
		public bool hasNormal { get; set; }
		public bool[] hasUV { get; set; }
		public bool hasTangent { get; set; }
		public bool hasColor { get; set; }
	}
}
