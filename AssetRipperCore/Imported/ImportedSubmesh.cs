using System.Collections.Generic;

namespace AssetRipper.Imported
{
	public class ImportedSubmesh
	{
		public List<ImportedFace> FaceList { get; set; }
		public string Material { get; set; }
		public int BaseVertex { get; set; }
	}
}
