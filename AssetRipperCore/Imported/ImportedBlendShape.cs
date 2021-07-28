using System.Collections.Generic;

namespace AssetRipper.Imported
{
	public class ImportedBlendShape
	{
		public string ChannelName;
		public List<ImportedKeyframe<float>> Keyframes = new List<ImportedKeyframe<float>>();
	}
}
