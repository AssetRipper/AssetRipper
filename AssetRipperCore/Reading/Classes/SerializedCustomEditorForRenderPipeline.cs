using AssetRipper.IO.Extensions;
using System.IO;

namespace AssetRipper.Reading.Classes
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
