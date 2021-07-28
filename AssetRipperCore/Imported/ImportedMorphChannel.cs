using System.Collections.Generic;

namespace AssetRipper.Imported
{
	public class ImportedMorphChannel
	{
		public string Name { get; set; }
		public List<ImportedMorphKeyframe> KeyframeList { get; set; }
	}
}
