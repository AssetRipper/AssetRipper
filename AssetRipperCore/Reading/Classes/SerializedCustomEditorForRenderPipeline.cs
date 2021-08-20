using AssetRipper.Core.IO.Extensions;
using System.IO;

namespace AssetRipper.Core.Reading.Classes
{
	public class SerializedCustomEditorForRenderPipeline
	{
		public string customEditorName;
		public string renderPipelineType;

		public SerializedCustomEditorForRenderPipeline(BinaryReader reader)
		{
			customEditorName = reader.ReadAlignedString();
			renderPipelineType = reader.ReadAlignedString();
		}
	}
}
